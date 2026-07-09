using System;
using UnityEngine;

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

    public static SubjectData[] TryParseSubjectLevels(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("[RemoteConfig] TryParse received empty JSON string.");
            return null;
        }

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
            catch (Exception ex)
            {
                Debug.LogWarning($"[RemoteConfig] Subject root-array parse failed: {ex.Message}");
            }
        }

        try
        {
            var wrapper = JsonUtility.FromJson<SubjectDataWrapper>(json);
            if (wrapper?.Data != null && wrapper.Data.Length > 0)
                return wrapper.Data;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[RemoteConfig] Subject JSON parse failed: {ex.Message}");
        }

        return null;
    }

    public static QuestEntry[] TryParseQuests(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("[RemoteConfig] TryParse received empty JSON string.");
            return null;
        }

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
            catch (Exception ex)
            {
                Debug.LogWarning($"[RemoteConfig] Quest root-array parse failed: {ex.Message}");
            }
        }

        try
        {
            var wrapper = JsonUtility.FromJson<QuestListWrapper>(json);
            if (wrapper?.quests != null && wrapper.quests.Length > 0)
                return wrapper.quests;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[RemoteConfig] Quest JSON parse failed: {ex.Message}");
        }

        return null;
    }

    public static QuestionsAnswerModel[] TryParseExamQuestions(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("[RemoteConfig] TryParse received empty JSON string.");
            return null;
        }

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
            catch (Exception ex)
            {
                Debug.LogWarning($"[RemoteConfig] Exam root-array parse failed: {ex.Message}");
            }
        }

        try
        {
            var wrapper = JsonUtility.FromJson<ExamQuestionWrapper>(json);
            if (wrapper?.questions != null && wrapper.questions.Length > 0)
                return wrapper.questions;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[RemoteConfig] Exam JSON parse failed: {ex.Message}");
        }

        return null;
    }
}