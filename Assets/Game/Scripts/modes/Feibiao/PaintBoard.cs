using DG.Tweening;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GridData
{
    public List<Vector2Int> positions;
    public int colorIndex;
    public int step;
}

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class PaintBoard:MonoBehaviour
    {
        [SerializeField] public Transform gridContainer;
        [SerializeField] private Button previewButton;
        [SerializeField] private Sprite[] filledSprites;
        [SerializeField] private Button nextStepButton;
        [SerializeField] private Button lastStepButton;
        [SerializeField] private Button importButton;
        [SerializeField] private RectTransform maskNode;
        [SerializeField] private RectTransform parentNode;
        [SerializeField] private Button prevStepButton;

        private List<GridData> history = new List<GridData>();
        private List<GridData> drawHistory = new List<GridData>();
        private int currentStep = 0;

        private float cellSize = 14f;
        private int rows = 35;
        private int cols = 35;

        private bool isAutoPlaying = false;
        private float playInterval = 0.01f;

        private readonly int COLORS_COUNT = 20;

        private Dictionary<string, GameObject> cellNodes = new Dictionary<string, GameObject>();

        void Start()
        {
            AdjustNodeHeight();
        }

        private void AdjustNodeHeight()
        {
            if (gridContainer == null) return;

            float gridWidth = cols * cellSize;
            float gridHeight = rows * cellSize;

            float x = -gridWidth / 2;
            float y = -gridHeight / 2;

            gridContainer.localPosition = new Vector3(x, y, 0);

            if (parentNode == null) return;

            RectTransform parentRect = parentNode.GetComponent<RectTransform>();
            float nodeHeight = Screen.height / 2 - parentNode.localPosition.y - 50;

            // 考虑安全区域
            Rect safeArea = Screen.safeArea;
            nodeHeight = nodeHeight - (Screen.height - safeArea.height - safeArea.y);
            if (nodeHeight > gridHeight)
            {
                nodeHeight = gridHeight;
            }
            Debug.Log("nodeHeight: " + nodeHeight);
            parentRect.sizeDelta = new Vector2(parentRect.sizeDelta.x, nodeHeight);
        }

        private void RedrawToStep(int step)
        {
            ClearAllCells();

            var sortedHistory = history.OrderBy(h => h.step).ToList();

            foreach (var item in sortedHistory)
            {
                if (item.step <= step)
                {
                    foreach (var pos in item.positions)
                    {
                        GameObject node = DrawCell(pos.x, pos.y, item.colorIndex);
                        if (node != null)
                        {
                            if (isAutoPlaying && item.step == step)
                            {
                                node.transform.localScale = Vector3.zero;
                                node.transform.DOScale(Vector3.one * 1.2f, 0.2f)
                                    .OnComplete(() =>
                                    {
                                        node.transform.DOScale(Vector3.one, 0.1f);
                                    });
                            }
                            var spriteRenderer = node.GetComponent<SpriteRenderer>();
                            if (spriteRenderer != null)
                            {
                                Color color = spriteRenderer.color;
                                color.a = 1f;
                                spriteRenderer.color = color;
                            }
                        }
                    }
                }
            }
        }

        private GameObject DrawCell(int x, int y, int colorIndex)
        {
            if (x < 0 || x >= cols || y < 0 || y >= rows) return null;

            string key = $"{x},{y}";
            if (!cellNodes.TryGetValue(key, out GameObject cellNode))
            {
                cellNode = new GameObject("Cell");
                Image image = cellNode.AddComponent<Image>();
                cellNode.layer = LayerMask.NameToLayer("UI");
                image.sprite = filledSprites[colorIndex];
                cellNode.transform.SetParent(gridContainer);
                //image.rectTransform.sizeDelta = new Vector2(filledSprites[colorIndex].rect.width, filledSprites[colorIndex].rect.height);
                image.SetNativeSize();

                float padding = 0;
                cellNode.transform.localScale = new Vector3(
                    (cellSize - padding * 2) / image.sprite.bounds.size.x,
                    (cellSize - padding * 2) / image.sprite.bounds.size.y,
                    1
                );

                cellNode.transform.localPosition = GetCellPosition(x, y);
                cellNodes[key] = cellNode;
            }

            return cellNode;
        }

        public Vector3 GetCellPosition(int x, int y)
        {
            return new Vector3(
                x * cellSize + cellSize / 2,
                y * cellSize + cellSize / 2,
                0
            );
        }

        private void SetupPreviewButton()
        {
            if (previewButton != null)
            {
                previewButton.onClick.AddListener(AutoPlayClick);
            }
        }

        private void AutoPlayClick()
        {
            if (isAutoPlaying)
            {
                StopPlay();
            }
            else
            {
                StartPlay();
            }
        }

        private void StartPlay()
        {
            isAutoPlaying = true;
            if (currentStep >= history.Count)
            {
                ClearAllCells();
                currentStep = 0;
            }

            if (previewButton != null)
            {
                var colors = previewButton.colors;
                colors.normalColor = Color.green;
                previewButton.colors = colors;
            }

            PlayNextStep();
        }

        private void DrawStepPosition(Vector2Int pos, int colorIndex)
        {
            GameObject node = DrawCell(pos.x, pos.y, colorIndex);
            if (node != null)
            {
                node.transform.localScale = Vector3.zero;
                node.transform.DOScale(Vector3.one * 1.2f, 0.2f)
                    .OnComplete(() => node.transform.DOScale(Vector3.one, 0.1f));
                Image image = node.GetComponent<Image>();
                if (image != null)
                {
                    Color color = image.color;
                    color.a = 1f;
                    image.color = color;
                }
            }

            if (!IsInViewport(pos.x, pos.y))
            {
                ScrollToCell(pos.x, pos.y);
            }
        }

        private void PlayNextStep()
        {
            if (!isAutoPlaying) return;

            var sortedHistory = history.OrderBy(h => h.step).ToList();
            if (currentStep >= sortedHistory.Count)
            {
                StopPlay();
                return;
            }

            var currentStepData = sortedHistory[currentStep];

            for (int i = 0; i < currentStepData.positions.Count; i++)
            {
                float delay = i * 0.05f;
                DOVirtual.DelayedCall(delay, () =>
                {
                    DrawStepPosition(currentStepData.positions[i], currentStepData.colorIndex);
                });
            }

            float stepDuration = currentStepData.positions.Count * 0.05f;
            DOVirtual.DelayedCall(stepDuration + playInterval, () =>
            {
                currentStep++;
                PlayNextStep();
            });
        }

        public (float duration, bool isFinished) DrawWithColor(int colorIndex, Action<Vector2Int, bool> callback)
        {
            var sortedHistory = drawHistory.OrderBy(h => h.step).ToList();
            int targetIndex = sortedHistory.FindIndex(item => item.colorIndex == colorIndex);

            if (targetIndex == -1)
            {
                Debug.LogWarning($"未找到颜色索引 {colorIndex} 对应的步骤");
                return (0, false);
            }

            var targetStep = sortedHistory[targetIndex];
            float delay = 0.04f;

            for (int i = 0; i < targetStep.positions.Count; i++)
            {
                int index = i;
                DOVirtual.DelayedCall(index * delay, () =>
                {
                    DrawStepPosition(targetStep.positions[index], colorIndex);
                    callback?.Invoke(targetStep.positions[index], index == targetStep.positions.Count - 1);
                });
            }

            drawHistory.Remove(targetStep);
            Debug.Log("targetStep: " + targetStep.positions.Count);

            return (targetStep.positions.Count * delay, drawHistory.Count == 0);
        }
        private void StopPlay()
        {
            isAutoPlaying = false;
            if (previewButton != null)
            {
                var colors = previewButton.colors;
                colors.normalColor = Color.white;
                previewButton.colors = colors;
            }
            //LeanTween.cancelAll();
            DOTween.PauseAll();
        }

        private void SetupNextStepButton()
        {
            if (nextStepButton != null)
            {
                nextStepButton.onClick.AddListener(PlayNextStepManually);
            }
        }

        private void PlayNextStepManually()
        {
            Debug.Log("playNextStepManually: " + currentStep);
            StopPlay();

            currentStep++;
            if (currentStep > history.Count)
            {
                currentStep = 1;
            }

            RedrawToStep(currentStep);
        }

        private void SetupPrevStepButton()
        {
            if (prevStepButton != null)
            {
                prevStepButton.onClick.AddListener(PlayPrevStepManually);
            }
        }

        private void PlayPrevStepManually()
        {
            StopPlay();

            currentStep--;

            if (currentStep < 1)
            {
                currentStep = history.Count;
            }

            RedrawToStep(currentStep);
        }

        private void SetupLastStepButton()
        {
            if (lastStepButton != null)
            {
                lastStepButton.onClick.AddListener(JumpToLastStep);
            }
        }

        private void JumpToLastStep()
        {
            if (isAutoPlaying) return;

            currentStep = history.Count;
            RedrawToStep(currentStep);
        }

        private void SetupImportButton()
        {
            if (importButton != null)
            {
                importButton.onClick.AddListener(ImportJson);
            }
        }

        private void ClearAllCells()
        {
            foreach (var node in cellNodes.Values)
            {
                Destroy(node);
            }
            cellNodes.Clear();
        }

        public void LoadGridData(GridDataWrapper jsonData)
        {
            try
            {
                var data = jsonData;// JsonUtility.FromJson<GridDataWrapper>(jsonData);
                if (!ValidateJsonData(data))
                {
                    Debug.LogWarning("无效的JSON格式");
                    return;
                }

                history.Clear();
                foreach (var step in data.s)
                {
                    history.Add(new GridData
                    {
                        positions = step.p.Select(p => new Vector2Int(p[0], p[1])).ToList(),
                        colorIndex = step.c,
                        step = step.s
                    });
                }

                drawHistory = new List<GridData>(history);
                currentStep = 0;
                RedrawToStep(currentStep);
                ResetPaintBoardPosition();
            }
            catch (Exception e)
            {
                Debug.LogError("JSON解析错误: " + e.Message);
            }
        }

        public void FixHistory(int targetSteps)
        {
            // 确保targetSteps为整数
            targetSteps = Mathf.FloorToInt(targetSteps);
            if (targetSteps <= 0 || history.Count == 0 || history.Count >= targetSteps)
            {
                Debug.Log("无需操作");
                return;
            }
            var redistributedHistory = RedistributeHistory(targetSteps, 15);
            drawHistory = redistributedHistory;
            Debug.Log($"drawHistory: {drawHistory.Count}");
        }

        private void ResetPaintBoardPosition()
        {
            if (gridContainer == null || maskNode == null) return;

            float maskHeight = maskNode.rect.height;
            float gridHeight = rows * cellSize;

            float minY = maskHeight - gridHeight / 2;
            float maxY = gridHeight / 2;

            minY = Mathf.Min(minY, maxY);
            var paintBoard = gridContainer.parent;
            if (paintBoard != null)
            {
                paintBoard.localPosition = new Vector3(0, minY, 0);
            }
        }

        private bool ValidateJsonData(GridDataWrapper data)
        {
            if (data == null || data.s == null)
            {
                Debug.LogWarning("数据为空");
                return false;
            }

            foreach (var step in data.s)
            {
                if (step.p == null)
                {
                    Debug.LogWarning($"步骤 {step.s} 的位置数据无效");
                    return false;
                }

                if (step.c < 0 || step.c >= COLORS_COUNT)
                {
                    Debug.LogWarning($"步骤 {step.s} 的颜色索引无效: {step.c}");
                    return false;
                }

                foreach (var pos in step.p)
                {
                    if (pos.Length != 2)
                    {
                        Debug.LogWarning($"步骤 {step.s} 的位置格式无效: {string.Join(",", pos)}");
                        return false;
                    }

                    if (!IsPointInGrid(pos[0], pos[1]))
                    {
                        Debug.LogWarning($"步骤 {step.s} 的位置超出网格范围: x={pos[0]}, y={pos[1]}");
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsPointInGrid(int x, int y)
        {
            return x >= 0 && x < cols && y >= 0 && y < rows;
        }

        private void ImportJson()
        {
            // 在Unity中，我们需要使用文件对话框
            string path = EditorUtility.OpenFilePanel("选择JSON文件", "", "json");
            if (string.IsNullOrEmpty(path)) return;

            //string jsonData = File.ReadAllText(path);
            //LoadGridData(jsonData);
        }

        private void ScrollToCell(int x, int y)
        {
            if (gridContainer == null || maskNode == null) return;

            float maskHeight = maskNode.rect.height;
            float gridHeight = rows * cellSize;
            var paintBoard = gridContainer.parent;
            if (paintBoard == null) return;

            float minY = maskHeight - gridHeight / 2;
            float maxY = gridHeight / 2;

            float cellY = y * cellSize - gridHeight / 2;
            float minGridY = -minY;
            float offsetY = cellY - minGridY;
            float targetY = minY - offsetY + maskHeight / 2;

            float clampedTargetY = Mathf.Clamp(targetY, minY, maxY);

            paintBoard.gameObject.transform.DOLocalMoveY( clampedTargetY, 0.2f)
                .SetEase(Ease.OutQuart);
        }

        private bool IsInViewport(int x, int y)
        {
            if (gridContainer == null || maskNode == null) return true;

            float maskHeight = maskNode.rect.height;
            float gridHeight = rows * cellSize;
            var paintBoard = gridContainer.parent;
            if (paintBoard == null) return true;

            float minY = maskHeight - gridHeight / 2;
            float maxY = gridHeight / 2;

            float cellY = y * cellSize - gridHeight / 2;
            float cellToMaskY = cellY + paintBoard.localPosition.y;

            return cellToMaskY >= 0 && cellToMaskY <= maskHeight;
        }

        public List<GridData> GetOperateSteps()
        {
            return drawHistory;
        }

        public void OnSaveAsPNG()
        {
            JumpToLastStep();
            SaveAsPNG();
        }

        public void SaveAsPNG()
        {
            // 创建RenderTexture
            RenderTexture rt = new RenderTexture(490, 490, 24);
            rt.Create();

            // 创建临时相机
            GameObject cameraObj = new GameObject("TempCamera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.targetTexture = rt;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0, 0, 0, 0);
            camera.orthographic = true;
            camera.orthographicSize = 245;
            camera.transform.position = new Vector3(0, 0, -10);

            // 渲染
            camera.Render();

            // 创建Texture2D并读取像素
            Texture2D tex = new Texture2D(490, 490, TextureFormat.RGBA32, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 490, 490), 0, 0);
            tex.Apply();

            // 保存为PNG
            byte[] bytes = tex.EncodeToPNG();
            string path = EditorUtility.SaveFilePanel("保存PNG", "", "grid.png", "png");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, bytes);
            }

            // 清理
            DestroyImmediate(cameraObj);
            DestroyImmediate(tex);
            rt.Release();
        }

        private List<GridData> RedistributeHistory(int targetSteps, int minStepLength)
        {
            var sortedHistory = history
                .Select(item => new GridData
                {
                    positions = new List<Vector2Int>(item.positions),
                    colorIndex = item.colorIndex,
                    step = item.step
                })
                .OrderBy(a => a.positions.Count)
                .ToList();

            var result = sortedHistory
                .Select(item => new GridData
                {
                    positions = new List<Vector2Int>(item.positions),
                    colorIndex = item.colorIndex,
                    step = item.step
                })
                .ToList();

            while (result.Count < targetSteps)
            {
                int currentIndex = result.Count - 1;
                var currentItem = result[currentIndex];
                if (currentItem.positions.Count < minStepLength)
                {
                    break;
                }

                int midPoint = Mathf.CeilToInt(currentItem.positions.Count / 2f);
                var firstHalf = currentItem.positions.Take(midPoint).ToList();
                var secondHalf = currentItem.positions.Skip(midPoint).ToList();

                result[currentIndex] = new GridData
                {
                    positions = firstHalf,
                    colorIndex = currentItem.colorIndex,
                    step = currentItem.step
                };

                result.Add(new GridData
                {
                    positions = secondHalf,
                    colorIndex = currentItem.colorIndex,
                    step = currentItem.step
                });

                result = result.OrderBy(a => a.positions.Count).ToList();
            }

            for (int i = 0; i < result.Count; i++)
            {
                result[i].step = i + 1;
            }

            return result;
        }

        [Serializable]
        public class GridDataWrapper
        {
            public float r;
            public float c;
            public List<StepData> s;
        }

        [Serializable]
        public class StepData
        {
            public List<int[]> p;
            public int c;
            public int s;
        }
    }
}
