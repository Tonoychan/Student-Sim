using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SubjectDataLoader : ISubjectDataLoader
{
    public GameEnums.MainSubjects Subject { get; }
    
    private List<SubjectData> _cachedData;
    private bool _isLoaded;

    private SubjectsDataSingle _localFallbackData;
    
    public SubjectDataLoader(GameEnums.MainSubjects subject, SubjectsDataSingle localFallbackData)
    {
        Subject = subject;
        _localFallbackData = localFallbackData;
    }

    public async UniTask LoadAsync()
    {
        //Check for cloud fetch
        
        //If could fetch fails load from the local SO
        _cachedData = new List<SubjectData>(_localFallbackData.Data);
        _isLoaded = true;

        await UniTask.Yield();
    }

    public IReadOnlyList<SubjectData> GetAllLevels()
    {
        EnsureLoaded();
        return _cachedData;
    }

    public SubjectData GetLevelData(int level)
    {
        EnsureLoaded();
        return _cachedData.Find(x=> x.subjectLevel == level);
    }
    
    public void EnsureLoaded()
    {
        if (!_isLoaded)
            throw new System.InvalidOperationException("Data not loaded yet.");
    }
    
    public string SubjectName => _localFallbackData.subjectName;
    public Sprite Icon => null;

}
