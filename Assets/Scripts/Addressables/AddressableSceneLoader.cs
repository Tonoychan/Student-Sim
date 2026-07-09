using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads the main game scene through Addressables.
/// </summary>
public static class AddressableSceneLoader
{
    static AsyncOperationHandle<SceneInstance> _currentSceneHandle;

    /// <summary>Loads scene_game. Unloads a previous instance if needed.</summary>
    public static async UniTask<bool> LoadGameSceneAsync()
    {
        if (_currentSceneHandle.IsValid())
        {
            await Addressables.UnloadSceneAsync(_currentSceneHandle);
            _currentSceneHandle = default;
        }

        try
        {
            _currentSceneHandle = Addressables.LoadSceneAsync(
                AddressableKeys.SceneGame,
                LoadSceneMode.Single);

            await _currentSceneHandle.ToUniTask();

            if (_currentSceneHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[Addressables] Scene load failed.");
                return false;
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Addressables] Scene load error: {ex.Message}");
            return false;
        }
    }
}
