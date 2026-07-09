using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contract for loading subject level data (used by SubjectService).
/// </summary>
public interface ISubjectDataLoader
{
    public IReadOnlyList<SubjectData> GetAllLevels();
    public SubjectData GetLevelData(int level);
    public void EnsureLoaded();
}
