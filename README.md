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

**Indian Student Sim** is a Unity-based simulator that puts you in the shoes of an Indian student navigating the pressure-cooker world of exams, subjects, and academic goals. Manage your subjects, answer exam questions, track your scores, and chase your academic dreams — all while surviving the grind.

> *Study hard. Score high. Make the family proud.*

---

## 🎮 Gameplay Overview

```
📚 Pick your Subjects
        ↓
🎯 Set your Goals
        ↓
📝 Sit the Exam
        ↓
✅ Select your Answers
        ↓
📊 See your Score
        ↓
🔁 Repeat until you top the class
```

| Feature | Description |
|---|---|
| 📖 **Subject System** | Choose from multiple subjects, each with their own question banks |
| 🎯 **Goal Tracking** | Set academic goals and track progress toward them |
| 📝 **Exam Engine** | Multiple-choice exam flow with scored results |
| 📊 **Score Board** | Per-subject score tracking and performance feedback |
| 🌟 **Custom Shaders** | Hand-crafted URP shaders and HLSL visual effects |

---

## 🏗️ Project Architecture

```
Assets/
├── Scripts/
│   ├── UI/
│   │   ├── SimulatorManager.cs         # Core game loop controller
│   │   ├── SimulatorUI.cs              # UI state management
│   │   ├── SelectAnswer.cs             # Answer selection logic
│   │   ├── SubjectReferenceOnButton.cs # Subject button binding
│   │   ├── SubjectUIReferenceOnButton.cs
│   │   └── SubjectScoreTextReferenceOnText.cs  # Score display
│   └── SO/  (ScriptableObjects)
│       ├── MainExamData.cs             # Exam question data asset
│       ├── SubjectsData.cs             # Subject configuration asset
│       └── GoalData.cs                 # Player goal data asset
└── Shaders/                            # Custom URP / HLSL shaders
```

### Design Patterns Used
- **ScriptableObject architecture** — data-driven exam/subject/goal configuration
- **MVC-style UI separation** — `SimulatorManager` (logic) ↔ `SimulatorUI` (view)
- **Component binding pattern** — UI elements reference data via typed reference scripts

---

## 🛠️ Tech Stack

| Area | Technology |
|---|---|
| Engine | Unity 6 (6000.3.10f1) |
| Language | C# 9.0 |
| Render Pipeline | Universal Render Pipeline (URP) |
| Shaders | ShaderLab · HLSL |
| UI | Unity uGUI · TextMesh Pro |
| Input | Unity Input System |
| IDE | JetBrains Rider |
| Target Platform | Android |

---

## 🚀 Getting Started

### Prerequisites

- [Unity Hub](https://unity.com/download) installed
- Unity Editor **6000.3.10f1** (exact version recommended)
- Android Build Support module installed

### Setup

```bash
# 1. Clone the repository
git clone https://github.com/Tonoychan/Student-Sim.git

# 2. Open Unity Hub → Add project → Select the cloned folder

# 3. Open with Unity 6000.3.10f1

# 4. Let Unity import all packages (first open may take a few minutes)

# 5. Open the main scene from Assets/Scenes/ and hit Play ▶
```

> ⚠️ **Note:** Make sure you open with the correct Unity version. Using a different version may cause URP shader compatibility issues.

---

## 📦 Packages & Dependencies

All packages are managed via Unity Package Manager and will resolve automatically on first open.

| Package | Purpose |
|---|---|
| Universal RP | Custom shader rendering |
| Shader Graph | Visual shader authoring |
| Input System | Touch & button input |
| TextMesh Pro | Crisp UI text rendering |
| Localization | Multi-language support |
| Timeline | Cutscene / sequence support |
| Visual Scripting | Rapid prototyping |
| 2D Sprite / Animation | Character & UI sprites |

---

## 🗺️ Roadmap

- [x] Core exam question flow
- [x] Subject selection system
- [x] Score tracking per subject
- [x] Goal data architecture
- [ ] Main menu & scene transitions
- [ ] Sound effects & background music
- [ ] Save/load system for progress
- [ ] Multiple difficulty levels
- [ ] Leaderboard / high score system

---

## 🤝 Contributing

This is a personal learning project but contributions and suggestions are welcome!

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
