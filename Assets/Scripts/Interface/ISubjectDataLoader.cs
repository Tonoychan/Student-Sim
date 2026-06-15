using System.Collections.Generic;
using UnityEngine;

public interface ISubjectDataLoader
{
    public IReadOnlyList<SubjectData> GetAllLevels();
    public SubjectData GetLevelData(int level);
    public void EnsureLoaded();
}
