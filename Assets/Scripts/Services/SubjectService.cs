using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SubjectService
{

    #region Subjects Data Loading

    private readonly Dictionary<GameEnums.MainSubjects, SubjectDataLoader> _loaders = new Dictionary<GameEnums.MainSubjects, SubjectDataLoader>();

    public void RegisterLoader(SubjectDataLoader loader)
    {
        _loaders[loader.Subject] = loader;
    }

    public async UniTask InitializeAsync()
    {
        var loadTasks = new List<UniTask>(_loaders.Count);
        foreach (var loader in _loaders.Values)
            loadTasks.Add(loader.LoadAsync());
        await UniTask.WhenAll(loadTasks);
    }

    #endregion


    #region Helper Functions

    public SubjectData GetLevelData(GameEnums.MainSubjects subject, int level)
    {
        return _loaders[subject].GetLevelData(level);
    }
    public IReadOnlyList<SubjectData> GetAllLevels(GameEnums.MainSubjects subject)
    {
        return _loaders[subject].GetAllLevels();
    }
    public bool HasSubject(GameEnums.MainSubjects subject)
    {
        return _loaders.ContainsKey(subject);
    }
    
    public string GetSubjectName(GameEnums.MainSubjects subject)
    {
        return _loaders[subject].SubjectName;
    }
    public Sprite GetSubjectIcon(GameEnums.MainSubjects subject)
    {
        return _loaders[subject].Icon; // or from a visual config dictionary
    }
    
    public int GetMaxLevel(GameEnums.MainSubjects subject)
    {
        var levels = _loaders[subject].GetAllLevels();
        if (levels == null || levels.Count == 0) return 1;
        int max = 1;
        foreach (var d in levels)
            if (d.subjectLevel > max) max = d.subjectLevel;
        return max;
    }

    #endregion
    
}
