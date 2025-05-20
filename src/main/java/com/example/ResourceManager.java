// 添加持久化状态管理
private StateStore stateStore;

// 添加限流相关属性
private final RateLimiter requestRateLimiter;
private final CircuitBreaker circuitBreaker;

// 系统负载级别枚举
private enum LoadLevel {
    NORMAL, MEDIUM, HIGH, CRITICAL
}

// 当前系统负载状态
private volatile LoadLevel currentLoadLevel = LoadLevel.NORMAL;

// 负载监控线程
private final ScheduledExecutorService loadMonitor = Executors.newSingleThreadScheduledExecutor();

// 在构造函数中初始化状态存储
public ResourceManager(Configuration config) {
    // ... 现有代码 ...
    this.stateStore = new StateStore(config.getStoragePath());
    
    // 启动时恢复状态
    if (config.isRecoveryEnabled()) {
        recoverFromLastState();
    }
    
    // 初始化限流器，每秒允许的最大请求数
    this.requestRateLimiter = RateLimiter.create(config.getMaxRequestsPerSecond());
    
    // 初始化熔断器
    this.circuitBreaker = CircuitBreaker.builder()
        .failureRateThreshold(config.getFailureRateThreshold())
        .waitDurationInOpenState(Duration.ofMillis(config.getWaitDurationMs()))
        .ringBufferSizeInHalfOpenState(config.getRingBufferSizeInHalfOpenState())
        .ringBufferSizeInClosedState(config.getRingBufferSizeInClosedState())
        .build();
    
    // 启动负载监控
    startLoadMonitoring(config.getLoadCheckIntervalMs());
}

// 定期保存状态
private void persistState() {
    ResourceState currentState = new ResourceState();
    currentState.setAllocatedResources(this.allocatedResources);
    currentState.setAvailableResources(this.availableResources);
    currentState.setResourceRequests(this.pendingRequests);
    
    try {
        stateStore.saveState(currentState);
        logger.info("资源管理器状态已持久化");
    } catch (IOException e) {
        logger.error("持久化资源状态失败", e);
    }
}

// 从上一个状态恢复
private void recoverFromLastState() {
    try {
        ResourceState savedState = stateStore.loadLastState();
        if (savedState != null) {
            this.allocatedResources = savedState.getAllocatedResources();
            this.availableResources = savedState.getAvailableResources();
            this.pendingRequests = savedState.getResourceRequests();
            logger.info("从持久化状态恢复成功");
        }
    } catch (Exception e) {
        logger.warn("恢复状态失败，将使用默认初始化", e);
        initializeDefaultState();
    }
}

// 启动负载监控
private void startLoadMonitoring(long intervalMs) {
    loadMonitor.scheduleAtFixedRate(() -> {
        updateLoadLevel();
    }, 0, intervalMs, TimeUnit.MILLISECONDS);
}

// 更新负载级别
private void updateLoadLevel() {
    double cpuUsage = getSystemCpuUsage();
    double memoryUsage = getSystemMemoryUsage();
    int pendingRequestsCount = this.pendingRequests.size();
    
    if (cpuUsage > 90 || memoryUsage > 90 || pendingRequestsCount > 1000) {
        setLoadLevel(LoadLevel.CRITICAL);
    } else if (cpuUsage > 70 || memoryUsage > 70 || pendingRequestsCount > 500) {
        setLoadLevel(LoadLevel.HIGH);
    } else if (cpuUsage > 50 || memoryUsage > 50 || pendingRequestsCount > 200) {
        setLoadLevel(LoadLevel.MEDIUM);
    } else {
        setLoadLevel(LoadLevel.NORMAL);
    }
}

// 设置负载级别并记录日志
private void setLoadLevel(LoadLevel newLevel) {
    if (newLevel != currentLoadLevel) {
        logger.info("系统负载级别从 {} 变更为 {}", currentLoadLevel, newLevel);
        currentLoadLevel = newLevel;
    }
}

// 修改资源分配方法，添加限流和熔断逻辑
public ResourceAllocationResponse allocateResource(ResourceRequest request) {
    // 应用限流
    if (!requestRateLimiter.tryAcquire()) {
        logger.warn("请求被限流: {}", request);
        return ResourceAllocationResponse.rejected("请求频率过高，请稍后重试");
    }
    
    // 应用熔断
    try {
        return circuitBreaker.executeSupplier(() -> performResourceAllocation(request));
    } catch (CallNotPermittedException e) {
        logger.error("熔断器开启，请求被拒绝: {}", request);
        return ResourceAllocationResponse.rejected("系统暂时无法处理请求，请稍后重试");
    }
}

// 实际执行资源分配的方法
private ResourceAllocationResponse performResourceAllocation(ResourceRequest request) {
    // 根据负载级别执行不同的资源分配策略
    switch (currentLoadLevel) {
        case NORMAL:
            // 正常分配策略
            return normalAllocationStrategy(request);
        
        case MEDIUM:
            // 中等负载策略 - 可能会降低分配的资源量
            return mediumLoadAllocationStrategy(request);
            
        case HIGH:
            // 高负载策略 - 显著减少分配的资源量
            return highLoadAllocationStrategy(request);
            
        case CRITICAL:
            // 极限负载策略 - 只处理高优先级请求，拒绝低优先级
            if (request.getPriority() > ResourcePriority.HIGH.getValue()) {
                return criticalLoadAllocationStrategy(request);
            } else {
                logger.warn("系统负载极高，拒绝低优先级请求: {}", request);
                return ResourceAllocationResponse.rejected("系统负载过高，暂时只处理高优先级请求");
            }
            
        default:
            return normalAllocationStrategy(request);
    }
}

// 不同负载下的资源分配策略实现
private ResourceAllocationResponse normalAllocationStrategy(ResourceRequest request) {
    // 完整资源分配逻辑
    // ... 现有代码 ...
}

private ResourceAllocationResponse mediumLoadAllocationStrategy(ResourceRequest request) {
    // 分配请求资源的80%
    ResourceRequest adjustedRequest = request.withResourceAmount(
        (int)(request.getResourceAmount() * 0.8)
    );
    return normalAllocationStrategy(adjustedRequest);
}

// 其他负载级别的分配策略...

// 关闭资源管理器
public void shutdown() {
    // ... 现有代码 ...
    
    // 停止负载监控
    loadMonitor.shutdown();
    try {
        if (!loadMonitor.awaitTermination(5, TimeUnit.SECONDS)) {
            loadMonitor.shutdownNow();
        }
    } catch (InterruptedException e) {
        loadMonitor.shutdownNow();
    }
} 