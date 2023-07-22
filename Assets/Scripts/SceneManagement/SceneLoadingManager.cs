using System;
using System.Collections;
using System.Collections.Generic;
using Kanbarudesu.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanbarudesu.SceneManagement
{
    public class SceneLoadingManager : SingletonPersistent<SceneLoadingManager>
    {
        public enum CloseCondition { Automatically, OnKeyPressed }
        [Header("Loading Screen")]
        [SerializeField] private LoadingScreenUI loadingScreenUI;
        [SerializeField] private CloseCondition closeCondition;
        [SerializeField] private KeyCode closeKey;

        [Header("Loading Data")]
        [Tooltip("Add extra time for loading time. For testing purpose")]
        [SerializeField] private float extraLoadingTime;
        [Tooltip("Will load the initial scene on awake. it's safe to leave it null")]
        [SerializeField] private SceneLocation initialSceneToLoad;

        public event Action OnLoadSceneStarted;
        public event Action OnLoadSceneComplete;

        public bool IsLoading { get; private set; }
        public int ProcessCount { get; private set; }
        public int CurrentProcessIndex { get; private set; }
        public float CurrentProcessProgress { get; private set; }
        public string CurrentProcessDescription { get; private set; }
        public float TotalProgress
        {
            get
            {
                float progress = ((float)CurrentProcessIndex - 1) / (float)ProcessCount;
                progress += CurrentProcessProgress / (float)ProcessCount;
                return progress;
            }
        }

        private bool waitingForClose;
        private Scene prevMainScene;
        private Scene nextMainScene;

        private void Start()
        {
            if (initialSceneToLoad != null)
            {
                initialSceneToLoad.Load();
            }
        }

        private void Update()
        {
            if (waitingForClose && Input.GetKeyDown(closeKey))
            {
                waitingForClose = false;
                OnLoadSceneComplete?.Invoke();
            }
        }

        private void NextProcess(string description)
        {
            CurrentProcessIndex++;
            CurrentProcessDescription = description;
            // loadingScreenUI.UpdateLoadingDescription(description);
            CurrentProcessProgress = 0f;
        }

        public void LoadScene(SceneLocation sceneLocation)
        {
            if (loadingScreenUI == null)
            {
                Debug.Log("Loading Screen UI reference is missing.");
                return;
            }

            if (sceneLocation.ShowLoadingScreen)
                loadingScreenUI.Show(sceneLocation);

            StartCoroutine(LoadScene_Co(sceneLocation));
        }

        private IEnumerator LoadScene_Co(SceneLocation sceneLocation)
        {
            CurrentProcessIndex = 0;
            yield return new WaitForEndOfFrame();
            IsLoading = true;

            OnLoadSceneStarted?.Invoke();

            List<AsyncOperation> sceneProcess = new();
            float startTime = Time.time;
            ProcessCount = sceneLocation.additiveScenesName.Count + 2;

            sceneProcess.Add(SceneManager.LoadSceneAsync(sceneLocation.SceneName, sceneLocation.MainSceneLoadSceneMode));
            foreach (var sceneName in sceneLocation.additiveScenesName)
            {
                sceneProcess.Add(SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive));
            }

            for (int i = 0; i < sceneProcess.Count; i++)
            {
                NextProcess("Loading Scene Data...");
                while (!sceneProcess[i].isDone)
                {
                    CurrentProcessProgress = sceneProcess[i].progress;
                    yield return null;
                }
            }

            NextProcess("Building the level...");
            if (extraLoadingTime > 0)
            {
                float waitTime = extraLoadingTime - (Time.time - startTime);
                float totalWaitTime = waitTime;
                while (waitTime > 0f)
                {
                    waitTime -= Time.deltaTime;
                    CurrentProcessProgress = 1 - (waitTime / totalWaitTime);
                    yield return null;
                }
            }

            NextProcess("Loading Finished");
            yield return new WaitForSeconds(0.1f);
            IsLoading = false;

            if (closeCondition == CloseCondition.Automatically)
            {
                OnLoadSceneComplete?.Invoke();
            }
            else
            {
                waitingForClose = true;
                loadingScreenUI.UpdateLoadingIndicatorText("Press " + closeKey.ToString().ToUpper() + "!");
            }
        }

        // private IEnumerator LoadScene(SceneLocation sceneLocation, Action onLoadingComplete)
        // {
        //     currentProcessIndex = 0;
        //     isLoading = true;
        //     yield return new WaitForEndOfFrame();

        //     OnLoadSceneStarted?.Invoke(SceneManager.GetActiveScene(), SceneManager.GetSceneByName(sceneLocation.SceneName));

        //     List<AsyncOperation> processes = new List<AsyncOperation>();
        //     float startTime = Time.time;
        //     processCount = sceneLocation.additiveScenesName.Count + 2;

        //     NextProcess("Loading " + sceneLocation.SceneName);

        //     AsyncOperation mainSceneProgress = SceneManager.LoadSceneAsync(sceneLocation.SceneName, LoadSceneMode.Additive);
        //     mainSceneProgress.allowSceneActivation = false;
        //     while (!mainSceneProgress.isDone)
        //     {
        //         currentProcessProgress = mainSceneProgress.progress;
        //         if (currentProcessProgress >= .9f)
        //         {
        //             processes.Add(mainSceneProgress);
        //             break;
        //         }
        //         yield return null;
        //     }

        //     for (int i = 0; i < sceneLocation.additiveScenesName.Count; i++)
        //     {
        //         string sceneName = sceneLocation.additiveScenesName[i];
        //         NextProcess("Loading " + sceneName);

        //         AsyncOperation additiveProcess = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        //         additiveProcess.allowSceneActivation = false;

        //         while (!additiveProcess.isDone)
        //         {
        //             currentProcessProgress = additiveProcess.progress;
        //             if (currentProcessProgress >= .9f)
        //             {
        //                 processes.Add(additiveProcess);
        //                 break;
        //             }
        //             yield return null;
        //         }
        //     }
        //     Debug.Log("passed here too") ;

        //     NextProcess("Building the level...");
        //     if (extraLoadingTime > 0)
        //     {
        //         float waitTime = extraLoadingTime - (Time.time - startTime);
        //         float totalWaitTime = waitTime;
        //         while (waitTime > 0f)
        //         {
        //             waitTime -= Time.deltaTime;
        //             currentProcessProgress = 1 - (waitTime / totalWaitTime);
        //             yield return null;
        //         }
        //     }

        //     foreach (var process in processes)
        //     {
        //         process.allowSceneActivation = true;
        //     }

        //     yield return new WaitForEndOfFrame();
        //     isLoading = false;
        //     NextProcess("Loading Finished");

        //     onLoadingComplete?.Invoke();
        //     OnLoadSceneComplete?.Invoke(SceneManager.GetActiveScene(), SceneManager.GetSceneByName(sceneLocation.SceneName));
        // }

        // private IEnumerator LoadScene(SceneLocation sceneLocation, Action onLoadingComplete)
        // {
        //     yield return new WaitForEndOfFrame();

        //     OnLoadSceneStarted?.Invoke(SceneManager.GetActiveScene(), SceneManager.GetSceneByName(sceneLocation.SceneName));
        //     float currentProcessProgress = 0;
        //     float startTime = Time.time;

        //     AsyncOperation sceneProgress = SceneManager.LoadSceneAsync(sceneLocation.SceneName, sceneLocation.LoadSceneMode);
        //     sceneProgress.allowSceneActivation = false;

        //     while (!sceneProgress.isDone)
        //     {
        //         currentProcessProgress = sceneProgress.progress;
        //         loadingScreenUI.UpdateLoadingProgress("Loading " + sceneLocation.Name, currentProcessProgress);
        //         if (currentProcessProgress == .9f) break;
        //         yield return null;
        //     }

        //     if (extraLoadingTime > 0)
        //     {
        //         float waitTime = extraLoadingTime - (Time.time - startTime);
        //         float totalWaitTime = waitTime;
        //         while (waitTime > 0f)
        //         {
        //             waitTime -= Time.deltaTime;
        //             currentProcessProgress = 1 - (waitTime / totalWaitTime);
        //             loadingScreenUI.UpdateLoadingProgress("Building the level...", currentProcessProgress);
        //             yield return null;
        //         }
        //     }

        //     yield return new WaitForEndOfFrame();
        //     sceneProgress.allowSceneActivation = true;
        //     loadingScreenUI.UpdateLoadingProgress("Loading Finished", 1);

        //     onLoadingComplete?.Invoke();
        //     OnLoadSceneComplete?.Invoke(SceneManager.GetActiveScene(), SceneManager.GetSceneByName(sceneLocation.SceneName));
        // }
    }
}
