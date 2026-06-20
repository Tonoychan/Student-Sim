<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:FF6B35,50:F7C59F,100:1A1A2E&height=200&section=header&text=Student%20Sim&fontSize=52&fontColor=ffffff&fontAlignY=38&desc=A%20Portfolio%20Project&descAlignY=58&descSize=18&animation=fadeIn" width="100%"/>

<br/>

![Unity](https://img.shields.io/badge/Unity-6.3-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-9.0-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Android-3DDC84?style=for-the-badge&logo=android&logoColor=white)
![WebGL](https://img.shields.io/badge/Platform-WebGL-FF6B35?style=for-the-badge&logo=webgl&logoColor=white)
![URP](https://img.shields.io/badge/Render-URP-FF6B35?style=for-the-badge&logo=unity&logoColor=white)
![Status](https://img.shields.io/badge/Status-In%20Development-yellow?style=for-the-badge)

### 🎮 [Play in Browser](https://play.unity.com/en/games/0cc2ac5f-1f8f-4d27-8252-b06e62443b78/student-sim)

</div>

---

## 🎓 About The Game

**Student Sim** is a simulation that puts you in the shoes of a student, where your choices of action affect subject score, stamina, and exams.

> *Study hard. Manage your energy. Score high.*

---

## 🕹️ How to Play

- Each day your stamina refreshes and you get **12 interactions** to spend.
- Use your interactions wisely — choosing subjects increases your score, but every action costs stamina.
- Keep an eye on stamina: the day ends as soon as **either stamina or interactions hit 0**.
- On exam days, answer the questions correctly — your final score is based on your subject score and your number of correct answers.

---

## 🎮 Gameplay Overview

```
🔐 Login (guest or restored session)
        ↓
📋 Choose term length or Continue saved game
        ↓
📅 Daily loop — pick subjects, spend stamina & interactions
        ↓
📝 Exam days — multiple-choice exams (6 questions each)
        ↓
📊 Day summary → next day until term ends
        ↓
🏆 Term result — subject scores + exam multiplier
```

| Feature | Description | Done |
|---|---|---|
| 📖 **Subject System** | Six academic subjects (Math, History, Science, Geography, Arts, Computer) plus Rest — each driven by ScriptableObject data | ✅ |
| ⚡ **Stamina & Interactions** | 100 stamina and 12 daily interactions; the day ends when either runs out | ✅ |
| 📝 **Exam Engine** | Scheduled exam days with multiple-choice questions and per-exam scoring | ✅ |
| 🎯 **Quest System** | Deadline-based quests with score targets and gold rewards | ✅ |
| 📊 **Term Scoring** | Academic base score multiplied by overall exam performance at term end | ✅ |
| 💾 **Save / Continue** | Progress persisted locally (PlayerPrefs); cloud save provider scaffolded | ✅ |
| ☁️ **Unity Gaming Services** | Guest authentication, analytics, and remote config integration | ✅ |
| 🛠️ **Remote Config (live tuning)** | Configure term/exam/economy values remotely without a build | ⬜ |
| 🖼️ **Addressables + CCD** | Subject images and popups delivered via Addressables and Cloud Content Delivery | ⬜ |
| 🔒 **Cloud Code (server-authoritative gold)** | Gold coin additions validated server-side via Unity Cloud Code | ⬜ |
| ☁️ **Cloud Save** | Player data synced via Unity Cloud Save | ⬜ |
| 🏆 **Leaderboard** | Integrated leaderboard for high scores | ⬜ |
| 📆 **Extended Term Lengths** | 120 / 360-day terms with supporting data | ⬜ |
| 🤖 **AI Class Standings** | Runtime list of AI players showing class standing each day | ⬜ |
| 🛍️ **Store + Ads + IAP** | Ads to boost score multiplier; simple IAP for Gold | ⬜ |

> *Done column reflects current progress — will be corrected as features land.*

---

## 🏗️ Project Architecture

```
Assets/
├── Scenes/
│   ├── LoginScene.unity          # UGS init + guest login
│   ├── SelectionScene.unity      # New game / continue
│   └── SampleScene.unity         # Main gameplay loop
├── ScriptableObjectsData/        # GameConfig, subjects, exams, quests
├── Scripts/
│   ├── Bootstrap/
│   │   ├── UnityService.cs           # UGS initialization & login flow
│   │   └── GameSessionContext.cs     # New game / continue session state
│   ├── Controller/
│   │   └── GameController.cs           # Wires services + event handlers
│   ├── Services/
│   │   ├── DayCycleService.cs          # Day start/end, exam days, term completion
│   │   ├── ExamService.cs              # Exam question flow
│   │   ├── PlayerStateService.cs       # Stamina, scores, levels, interactions
│   │   ├── PlayerSaveService.cs        # Save/load orchestration
│   │   ├── QuestService.cs             # Quest evaluation & rewards
│   │   ├── SubjectService.cs           # Subject data access
│   │   ├── SubjectSelectionService.cs  # Daily subject picks
│   │   ├── TermScoreCalculator.cs      # Final term grade formula
│   │   └── ...                         # Currency, interaction, config loaders
│   ├── UI/ & AdvanceUI/                # Scene & gameplay UI (exam, stats, quests, results)
│   ├── Saving/                         # PlayerSaveData, ISaveProvider, providers
│   ├── ScriptableObjectScripts/        # GameConfigSO, MainExam, SubjectsDataSingle, QuestData
│   ├── UGS/                            # Auth, analytics, remote config
│   └── GameEvents.cs                   # Static event bus between services & UI
└── Art/                                # UI textures & sprites
```

### Design Patterns Used
- **Service-oriented architecture** — gameplay logic split into focused services, orchestrated by `GameController`
- **ScriptableObject data** — subjects, exams, quests, and term config are asset-driven
- **Event-driven UI** — `GameEvents` decouples services from UI presenters
- **Save provider abstraction** — `ISaveProvider` with local and cloud implementations
- **Async with UniTask** — UGS init, login, and scene transitions

---

## 🛠️ Tech Stack

| Area | Technology |
|---|---|
| Engine | Unity 6.3 (6000.3.10f1) |
| Language | C# 9.0 |
| Render Pipeline | Universal Render Pipeline (URP) |
| UI | Unity uGUI · TextMesh Pro |
| Input | Unity Input System |
| Async | UniTask (Cysharp) |
| Backend | Unity Gaming Services — Authentication, Analytics, Remote Config |
| IDE | JetBrains Rider · Visual Studio |
| Target Platforms | Android · WebGL |

---

## 🗺️ Roadmap

**MVP**

- ✅ Core exam question flow
- ✅ Subject selection & daily interaction loop
- ✅ Stamina, levels, and per-subject score tracking
- ✅ Quest system with deadlines and rewards
- ✅ Day cycle with scheduled exam days
- ✅ Term scoring and result screen
- ✅ Login, selection, and scene transitions
- ✅ Save / continue (local persistence)
- ✅ Unity Gaming Services integration (auth, analytics, remote config)

**MVP done — next: Meta Features**

- ⬜ Unity Remote Config configuration
- ⬜ Unity Addressables + CCD for subject images and popups
- ⬜ Unity Cloud Code for server-authoritative gold coin addition
- ⬜ Unity Cloud Save for player data
- ⬜ Leaderboard integration
- ⬜ 120 / 360-day term lengths with supporting data
- ⬜ Runtime AI player list showing class standing each day
- ⬜ Store + Ads to boost score multiplier, IAP for Gold
- ⬜ Sound effects & background music
- ⬜ Localization

---

## 👨‍💻 Author

**Tonoy Chakraborty**

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0A66C2?style=flat-square&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/tonoy-chakraborty-9b703097/)
[![Email](https://img.shields.io/badge/Email-EA4335?style=flat-square&logo=gmail&logoColor=white)](mailto:tonoychan55@gmail.com)

---

<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:1A1A2E,50:F7C59F,100:FF6B35&height=120&section=footer" width="100%"/>

*Built with Unity 6 · Made with ☕ and exam stress*

</div>
