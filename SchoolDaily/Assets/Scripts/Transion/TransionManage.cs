using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace SchoolD.Transition
{
    public class TransionManage : MonoBehaviour
    {
        public String startSceneName = string.Empty;
        private CanvasGroup fadeCanvaGroup;

        private bool isfade = false;

        private void OnEnable()
        {
            EventHandler.TransitionEvent += OnTransitionEvent;
        }
        private void OnDisable()
        {
            EventHandler.TransitionEvent -= OnTransitionEvent;
        }

        private void OnTransitionEvent(String sceneToGo, Vector3 positionToGo)
        {
            if (!isfade)
                StartCoroutine(Transition(sceneToGo, positionToGo));
        }

        private void Start()
        {
            StartCoroutine(StartSequence());
        }

        IEnumerator StartSequence()
        {
            int slot = SaveManager.Instance.currentSlot;

            // 读取是否是新游戏
            bool isNewGame = PlayerPrefs.GetInt("IsNewGame_" + slot, 0) == 1;

            if (isNewGame)
            {
                // 播放动画
                yield return BeginAnimManager.Instance.ShowAcceptTanceLetter();//录取通知书开场动画
                yield return StartCoroutine(LoadSceneSetActive(startSceneName, true));
                yield return BeginAnimManager.Instance.PlayNewBeginAnim();//公交车开场动画

                // 清除标记（避免重复播放）
                PlayerPrefs.DeleteKey("IsNewGame_" + slot);
                PlayerPrefs.Save();
            }
            else
            {
                BeginAnimManager.Instance.SetPlayerPosition();
                yield return StartCoroutine(LoadSceneSetActive(startSceneName, true));
            }

            fadeCanvaGroup = FindObjectOfType<CanvasGroup>();
            fadeCanvaGroup.alpha = 0;
        }


        /// <summary>
        /// 场景切换
        /// </summary>
        /// <param name="sceneName">目标场景</param>
        /// <param name="targetPosition">目标位置</param>
        private IEnumerator Transition(String sceneName, Vector3 targetPosition)
        {
            // Canvas parentCanvas = fadeCanvaGroup.GetComponentInParent<Canvas>();
            // parentCanvas.sortingOrder = 100;
            BlackScreenManager.Instance.TransionBlackScreenSortOrder(100);

            EventHandler.CallBeforeSceneUnLoadEvent();
            BlackScreenManager.Instance.SetText("Loading");
            yield return BlackScreenManager.Instance.FadeIn(0.5f, false);

            // yield return Fade(1);

            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            Debug.Log("加载：" + sceneName);

            yield return LoadSceneSetActive(sceneName);

            EventHandler.CallMoveToPositionEvent(targetPosition);//在场景加载好后移动任人物

            yield return new WaitForSeconds(Settings.blackoutDuration); //黑屏停留时间

            yield return BlackScreenManager.Instance.FadeOut(0.5f, false);
            //yield return Fade(0);

            //parentCanvas.sortingOrder = 0;
            BlackScreenManager.Instance.TransionBlackScreenSortOrder(0);

            EventHandler.CallAfterScenLoadEvent();

        }
        private IEnumerator LoadSceneSetActive(String sceneName, bool orign = false)//第二个参数为默认不是初始场景
        {
            // Debug.Log("加载1：" + sceneName);
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);//异步加载，叠加场景
            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            //Debug.Log(newScene.name);
            while (!newScene.isLoaded)
            {
                yield return null;
            }
            SceneManager.SetActiveScene(newScene);
            //Debug.Log("当前激活场景：" + SceneManager.GetActiveScene().name);
            if (orign == true)
            {
                EventHandler.CallAfterScenLoadEvent(); // 触发事件并标记初始场景加载
            }

        }
        /// <summary>
        /// 淡入淡出场景
        /// </summary>
        /// <param name="targetAlpha">目标透明度</param>
        private IEnumerator Fade(float targetAlpha)
        {
            isfade = true;

            fadeCanvaGroup.blocksRaycasts = true;

            float speed = Mathf.Abs(fadeCanvaGroup.alpha = targetAlpha) / Settings.fadeDuration;
            while (!Mathf.Approximately(fadeCanvaGroup.alpha, targetAlpha))
            {
                fadeCanvaGroup.alpha = Mathf.MoveTowards(fadeCanvaGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }

            isfade = false;

            fadeCanvaGroup.blocksRaycasts = false;
        }

    }

}
