using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Loads subject icon sprites from Addressables and hands them to SubjectService.
/// </summary>
public class SubjectVisualService
{
    public static SubjectVisualService Instance { get; private set; }

    readonly Dictionary<GameEnums.MainSubjects, Sprite> _icons = new();
    readonly List<AsyncOperationHandle<Sprite>> _handles = new();
    bool _isLoaded;

    public SubjectVisualService()
    {
        Instance = this;
    }

    public bool IsLoaded => _isLoaded;

    /// <summary>Loads all subject icons in parallel.</summary>
    public async UniTask LoadAllAsync()
    {
        if (_isLoaded)
            return;

        var subjects = new[]
        {
            GameEnums.MainSubjects.Math,
            GameEnums.MainSubjects.History,
            GameEnums.MainSubjects.Science,
            GameEnums.MainSubjects.Geography,
            GameEnums.MainSubjects.Arts,
            GameEnums.MainSubjects.Computer,
            GameEnums.MainSubjects.Rest,
            GameEnums.MainSubjects.Work,
            GameEnums.MainSubjects.Exercise,
        };

        var tasks = new List<UniTask>(subjects.Length);

        foreach (var subject in subjects)
        {
            string address = AddressableKeys.GetSubjectIconAddress(subject);
            if (string.IsNullOrEmpty(address))
                continue;

            tasks.Add(LoadIconAsync(subject, address));
        }

        await UniTask.WhenAll(tasks);
        _isLoaded = true;
    }

    async UniTask LoadIconAsync(GameEnums.MainSubjects subject, string address)
    {
        try
        {
            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(address);
            _handles.Add(handle);

            Sprite sprite = await handle.ToUniTask();
            if (sprite != null)
                _icons[subject] = sprite;
        }
        catch (System.Exception)
        {
        }
    }

    /// <summary>Returns the loaded icon, or null if not found.</summary>
    public Sprite GetIcon(GameEnums.MainSubjects subject)
    {
        return _icons.TryGetValue(subject, out Sprite sprite) ? sprite : null;
    }

    /// <summary>Frees addressable handles when no longer needed.</summary>
    public void Release()
    {
        foreach (var handle in _handles)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }
        _handles.Clear();
        _icons.Clear();
        _isLoaded = false;
    }
}
