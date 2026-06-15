<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:FF6B35,50:F7C59F,100:1A1A2E&height=200&section=header&text=Indian%20Student%20Sim&fontSize=52&fontColor=ffffff&fontAlignY=38&desc=A%20Unity%20Exam%20Simulator%20Experience&descAlignY=58&descSize=18&animation=fadeIn" width="100%"/>

<br/>

![Unity](https://img.shields.io/badge/Unity-6000.3-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-9.0-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Android-3DDC84?style=for-the-badge&logo=android&logoColor=white)
![URP](https://img.shields.io/badge/Render-URP-FF6B35?style=for-the-badge&logo=unity&logoColor=white)
![Status](https://img.shields.io/badge/Status-In%20Development-yellow?style=for-the-badge)

</div>

---

## 🎓 About The Game

**Indian Student Sim** is a Unity-based term simulator that puts you in the shoes of an Indian student navigating exams, subjects, stamina, and academic goals across a full school term.

Each day you pick subjects, manage limited interactions and stamina, sit scheduled exams, complete quests, and build scores across six core subjects. Finish the term to see your final grade — boosted by exam performance.

> *Study hard. Manage your energy. Score high. Make the family proud.*

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

| Feature | Description |
|---|---|
| 📖 **Subject System** | Six academic subjects (Math, History, Science, Geography, Arts, Computer) plus Rest — each driven by ScriptableObject data |
| ⚡ **Stamina & Interactions** | 100 stamina and 12 daily interactions; the day ends when either runs out |
| 📝 **Exam Engine** | Scheduled exam days with multiple-choice questions and per-exam scoring |
| 🎯 **Quest System** | Deadline-based quests with score targets and gold rewards |
| 📊 **Term Scoring** | Academic base score multiplied by overall exam performance at term end |
| 💾 **Save / Continue** | Progress persisted locally (PlayerPrefs); cloud save provider scaffolded |
| ☁️ **Unity Gaming Services** | Guest authentication, analytics, and remote config integration |

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
| Engine | Unity 6 (6000.3.10f1) |
| Language | C# 9.0 |
| Render Pipeline | Universal Render Pipeline (URP) |
| UI | Unity uGUI · TextMesh Pro |
| Input | Unity Input System |
| Async | UniTask (Cysharp) |
| Backend | Unity Gaming Services — Authentication, Analytics, Remote Config |
| IDE | JetBrains Rider · Visual Studio |
| Target Platform | Android |

---

## 🚀 Getting Started

### Prerequisites

- [Unity Hub](https://unity.com/download) installed
- Unity Editor **6000.3.10f1** (exact version recommended)
- Android Build Support module (for mobile builds)
- Unity Gaming Services project linked (for auth, analytics, and remote config in `LoginScene`)

### Setup

```bash
# 1. Clone the repository
git clone https://github.com/Tonoychan/Student-Sim.git

# 2. Open Unity Hub → Add project → Select the cloned folder

# 3. Open with Unity 6000.3.10f1

# 4. Let Unity import all packages (first open may take a few minutes)

# 5. Open LoginScene (first scene in Build Settings) and hit Play ▶
```

**Scene flow:** `LoginScene` → `SelectionScene` → `SampleScene`

> ⚠️ **Note:** Use Unity **6000.3.10f1** to avoid package and URP mismatches. UGS features require a configured Unity project; the game falls back gracefully if services fail to initialize.

---

## 📦 Packages & Dependencies

All packages resolve via Unity Package Manager on first open.

| Package | Purpose |
|---|---|
| Universal RP | Rendering pipeline |
| Input System | Touch & button input |
| TextMesh Pro | UI text |
| UniTask | Async/await for services and scene flow |
| Unity Authentication | Guest login & session restore |
| Unity Analytics | Gameplay event tracking |
| Unity Remote Config | Live term/exam configuration |
| Timeline | Sequence support |
| Visual Scripting | Prototyping |
| 2D Sprite / Animation | UI & sprite assets |

---

## 🗺️ Roadmap

- [x] Core exam question flow
- [x] Subject selection & daily interaction loop
- [x] Stamina, levels, and per-subject score tracking
- [x] Quest system with deadlines and rewards
- [x] Day cycle with scheduled exam days
- [x] Term scoring and result screen
- [x] Login, selection, and scene transitions
- [x] Save / continue (local persistence)
- [x] Unity Gaming Services integration (auth, analytics, remote config)
- [ ] Sound effects & background music
- [ ] Longer term lengths (120 / 360 days)
- [ ] Cloud save fully wired to gameplay
- [ ] Leaderboard / high score system
- [ ] Localization

---

## 🤝 Contributing

This is a personal learning project, but contributions and suggestions are welcome!

1. Fork the repo
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m 'Add your feature'`
4. Push to the branch: `git push origin feature/your-feature`
5. Open a Pull Request

---

## 👨‍💻 Author

**Tonoy Chakraborty** — Unity Engineer with 9+ years in mobile game development

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0A66C2?style=flat-square&logo=linkedin&logoColor=white)](https://linkedin.com/in/tonoy-chakraborty)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=flat-square&logo=github&logoColor=white)](https://github.com/Tonoychan)
[![Email](https://img.shields.io/badge/Email-EA4335?style=flat-square&logo=gmail&logoColor=white)](mailto:tonoychan55@gmail.com)

---

<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:1A1A2E,50:F7C59F,100:FF6B35&height=120&section=footer" width="100%"/>

*Built with Unity 6 · Made with ☕ and exam stress*

</div>
