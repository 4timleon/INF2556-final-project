using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    [SerializeField] private LoadingOverlayController _overlay;

    [Header("Bypass Settings")]
    [SerializeField] private List<string> _bypassedSceneNames = new List<string> { "MainMenu" };

    private CancellationTokenSource _lifetimeCts;
    private bool _isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _lifetimeCts = new CancellationTokenSource();
        _overlay.SetInstant(false);
    }

    private void Start()
    {
        if (_isTransitioning) return;

        Scene activeScene = SceneManager.GetActiveScene();
        if (IsBypassedScene(activeScene.name, activeScene.buildIndex))
        {
            _ = RunInitializationPhasesAsync(activeScene, _lifetimeCts.Token);
            return;
        }

        _ = InitializeCurrentSceneAsync();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (_lifetimeCts != null)
        {
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
            _lifetimeCts = null;
        }
    }

    private async Awaitable InitializeCurrentSceneAsync()
    {
        _isTransitioning = true;

        try
        {
            Scene activeScene = SceneManager.GetActiveScene();
            _overlay.SetInstant(true);

            await RunInitializationPhasesAsync(activeScene, _lifetimeCts.Token);

            await Awaitable.NextFrameAsync(_lifetimeCts.Token);
            await Awaitable.NextFrameAsync(_lifetimeCts.Token);

            await _overlay.FadeOutAsync(_lifetimeCts.Token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    //Scene Transition
    public async Awaitable<bool> TransitionToSceneAsync(string sceneName, bool useOverlay = true, CancellationToken ct = default)
    {
        if (_isTransitioning)
        {//If it is already transitioning then ignore the function and return false
            Debug.LogWarning("transitiion requested while another one is happening. Ignoring");
            return false;
        }

        if (_lifetimeCts != null && ct.CanBeCanceled)
        {
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, _lifetimeCts.Token))
            {
                await RunTransitionAsync(sceneName, -1, useOverlay, linkedCts.Token);
            }
        }
        else
        {
            await RunTransitionAsync(sceneName, -1, useOverlay, _lifetimeCts?.Token ?? ct);
        }

        return true;
    }

    private async Awaitable RunTransitionAsync(string sceneName, int buildIndex, bool useOverlay, CancellationToken ct)
    {
        _isTransitioning = true;

        //EventSystem
        EventSystem.current.enabled = false;

        //Should the transition have an overlay?
        bool displayVisuals = useOverlay && !IsBypassedScene(sceneName, buildIndex);

        try
        {
            _overlay.SetLoadingProgress(0f);
            Time.timeScale = 0f;

            //Fade in the overlay
            if (_overlay != null && displayVisuals)
            {//Overlay exists and should show the overlay
                await _overlay.FadeInAsync(ct);
            }

            Scene oldScene = SceneManager.GetActiveScene();

            //Load Scene Additively
            AsyncOperation loadOp = sceneName != null ? SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive) : SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);

            loadOp.allowSceneActivation = false; //Don't activate yet

            //Load 90% in the background
            while (loadOp.progress < 0.9f)
            {
                ct.ThrowIfCancellationRequested();
                _overlay.SetLoadingProgress(loadOp.progress);
                await Awaitable.NextFrameAsync(ct); //Wait one Frame, to prevent the game from freezing
            }

            _overlay.SetLoadingProgress(0.9f);

            loadOp.allowSceneActivation = true; //Now activate

            //Wait until Scene is fully loaded
            while (!loadOp.isDone)
            {
                ct.ThrowIfCancellationRequested();
                _overlay.SetLoadingProgress(loadOp.progress);
                await Awaitable.NextFrameAsync(ct); //Wait one Frame, to prevent the game from freezing
            }

            //Sets the new Scene as active Main Scene and unloads the old Scene
            Scene newScene = sceneName != null ? SceneManager.GetSceneByName(sceneName) : SceneManager.GetSceneByBuildIndex(buildIndex);
            SceneManager.SetActiveScene(newScene);

            if (oldScene.IsValid() && oldScene != newScene)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(oldScene);

                if (unloadOp != null)
                {
                    while (!unloadOp.isDone)
                    {
                        ct.ThrowIfCancellationRequested();
                        await Awaitable.NextFrameAsync(ct);
                    }
                }
            }

            //Run Systems by Priority 
            await RunInitializationPhasesAsync(newScene, ct);
            _overlay.SetLoadingProgress(1f);

            //Wait 2 Frames so that Physics, Render and UI can fully stabilize
            await Awaitable.NextFrameAsync(ct);
            await Awaitable.NextFrameAsync(ct);

            Time.timeScale = 1f;

            //Fade out the overlay
            if (_overlay != null && displayVisuals)
            {
                await _overlay.FadeOutAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        finally
        {//Set _isTransitioning always to false at the end
            _isTransitioning = false;
        }
    }

    //Run all InitializeAwake and InitializeStart functions by priority
    private async Awaitable RunInitializationPhasesAsync(Scene scene, CancellationToken ct)
    {
        var awakeSystems = new List<IInitializeAwake>();
        var startSystems = new List<IInititalizeStart>();

        GameObject[] rootObjects = scene.GetRootGameObjects();

        //Goes through all the root GameObjects and collects all the Components which implement the IInitializeAwake or IInititalizeStart interfaces
        foreach (GameObject root in rootObjects)
        {
            if (root == null) continue;

            awakeSystems.AddRange(root.GetComponentsInChildren<IInitializeAwake>(true));
            startSystems.AddRange(root.GetComponentsInChildren<IInititalizeStart>(true));
        }

        var sortedAwakeSystems = awakeSystems.OrderByDescending(s => s.AwakePriority).ToList();
        var sortedStartSystems = startSystems.OrderByDescending(s => s.StartPriority).ToList();

        for (int i = 0; i < sortedAwakeSystems.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            await sortedAwakeSystems[i].InitializeAwake(ct);
        }

        for (int i = 0; i < sortedStartSystems.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            await sortedStartSystems[i].InititalizeStart(ct);
        }
    }

    //Overlay Exclusion for a faster switch to a Scene
    private bool IsBypassedScene(string sceneName, int buildIndex)
    {
        //If the String is not Null or Empty then check if it is on the List of Scenes that should be excluded
        if (!string.IsNullOrEmpty(sceneName))
        {
            return _bypassedSceneNames.Contains(sceneName);
        }

        //If sceneName is Null or Empty then check the Scene with the buildIndex
        if (buildIndex > 0)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            string extractedName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            return _bypassedSceneNames.Contains(extractedName); //Is this name on the List of Scenes that should be excluded
        }

        return false;
    }
}
