using System;
using UnityEngine;

/// <summary>
/// Parses JSON strings from Remote Config into game data arrays.
/// </summary>
public static class RemoteConfigJsonHelper
{
    [Serializable]
    public class SubjectDataWrapper
    {
        public SubjectData[] Data;
    }

    [Serializable]
    public class QuestListWrapper
    {
        public QuestEntry[] quests;
    }

    [Serializable]
    public class ExamQuestionWrapper
    {
        public QuestionsAnswerModel[] questions;
    }

    /// <summary>Parses subject level JSON (wrapped object or raw array).</summary>
    public static SubjectData[] TryParseSubjectLevels(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        string trimmed = json.Trim();
        if (trimmed.StartsWith("["))
        {
            try
            {
                string wrapped = "{\"Data\":" + trimmed + "}";
                var rootWrapper = JsonUtility.FromJson<SubjectDataWrapper>(wrapped);
                if (rootWrapper?.Data != null && rootWrapper.Data.Length > 0)
                    return rootWrapper.Data;
            }
            catch (Exception)
            {
            }
        }

        try
        {
            var wrapper = JsonUtility.FromJson<SubjectDataWrapper>(json);
            if (wrapper?.Data != null && wrapper.Data.Length > 0)
                return wrapper.Data;
        }
        catch (Exception)
        {
        }

        return null;
    }

    /// <summary>Parses quest list JSON (wrapped object or raw array).</summary>
    public static QuestEntry[] TryParseQuests(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        string trimmed = json.Trim();
        if (trimmed.StartsWith("["))
        {
            try
            {
                string wrapped = "{\"quests\":" + trimmed + "}";
                var rootWrapper = JsonUtility.FromJson<QuestListWrapper>(wrapped);
                if (rootWrapper?.quests != null && rootWrapper.quests.Length > 0)
                    return rootWrapper.quests;
            }
            catch (Exception)
            {
            }
        }

        try
        {
            var wrapper = JsonUtility.FromJson<QuestListWrapper>(json);
            if (wrapper?.quests != null && wrapper.quests.Length > 0)
                return wrapper.quests;
        }
        catch (Exception)
        {
        }

        return null;
    }

    /// <summary>Parses exam question JSON (wrapped object or raw array).</summary>
    public static QuestionsAnswerModel[] TryParseExamQuestions(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        string trimmed = json.Trim();
        if (trimmed.StartsWith("["))
        {
            try
            {
                string wrapped = "{\"questions\":" + trimmed + "}";
                var rootWrapper = JsonUtility.FromJson<ExamQuestionWrapper>(wrapped);
                if (rootWrapper?.questions != null && rootWrapper.questions.Length > 0)
                    return rootWrapper.questions;
            }
            catch (Exception)
            {
            }
        }

        try
        {
            var wrapper = JsonUtility.FromJson<ExamQuestionWrapper>(json);
            if (wrapper?.questions != null && wrapper.questions.Length > 0)
                return wrapper.questions;
        }
        catch (Exception)
        {
        }

        return null;
    }
}
