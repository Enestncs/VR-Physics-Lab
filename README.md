
# VR Physics Lab

An experimental virtual reality learning environment that helps students explore physics through interactive, hands-on simulations.

> **Project status:** Prototype / In Development  
> **Target platform:** Meta Quest 3  
> **Engine:** Unity 6.3 LTS

---

## Overview

VR Physics Lab is an educational VR project exploring how immersive environments and interactive experimentation can support the teaching of fundamental physics concepts.

The project is designed with secondary-school students in mind, particularly learners around **Grades 9–10**. Instead of relying only on textbook explanations, students will be able to explore physical concepts in a virtual laboratory, manipulate objects, run experiments, and observe the results.

The initial development scope focuses on **mechanics**, with an emphasis on force and motion. The first milestone is a single virtual laboratory containing multiple experiments.

## Learning Goals

- Make abstract physics concepts easier to understand through visual and interactive experiences.
- Encourage learning through experimentation, observation, and discovery.
- Let students explore cause-and-effect relationships in a controlled virtual environment.
- Combine educational objectives with accessible interaction design.
- Explore the use of VR as a supplementary tool for physics education.

## Current Scope

The project is being developed iteratively. The following table distinguishes the current focus from ideas planned for later development.

| Area | Description | Status |
|---|---|---|
| VR laboratory | A shared virtual space for physics experiments | In Progress |
| Object interaction | Interacting with and manipulating experiment objects | In Progress |
| Mechanics | Initial focus on force and motion | In Progress |
| Multiple experiments | Several experiments within the first laboratory | Planned |
| Learning activities | Experiment-based tasks and guided discovery | Planned |
| Visual feedback | Clear visual representation of physical behaviour | Planned |

> This is a development roadmap, not a list of completed features. Statuses will be updated as implementation progresses.

## Technology Stack

| Technology | Purpose |
|---|---|
| [Unity 6.3 LTS](https://unity.com/releases/unity-6) (`6000.3.10f1`) | Game engine and development environment |
| C# | Gameplay and interaction logic |
| VR Interaction Framework | VR interaction foundation |
| Meta Quest 3 | Target standalone VR hardware |
| Blender | 3D asset creation, when needed |

## Getting Started

### Requirements

- Unity Hub
- Unity Editor `6000.3.10f1`
- Git
- The VR packages and dependencies configured by the project
- Meta Quest 3 for headset testing (optional for opening and inspecting the project)

For standalone Quest development, install the Android Build Support module, including the Android SDK & NDK Tools and OpenJDK, through Unity Hub.

### Clone the Repository

```bash
git clone https://github.com/Enestncs/VR-Physics-Lab.git
cd VR-Physics-Lab
```

### Open the Project

1. Open **Unity Hub**.
2. Select **Add project from disk**.
3. Choose the cloned `VR-Physics-Lab` directory.
4. Open the project with Unity Editor `6000.3.10f1`.
5. Allow Unity to resolve packages and import project assets.
6. Open the appropriate project scene and run it in the Editor.

### Test on Meta Quest 3

To build and test on a standalone headset:

1. Install the Android Build Support modules for the Unity Editor version used by the project.
2. Enable Developer Mode for your Meta Quest 3.
3. Connect the headset to your development computer using USB.
4. Allow USB debugging in the headset when prompted.
5. In Unity, select the Android build platform and configure the project's XR settings.
6. Build and deploy the project to the headset.

> **Note:** Exact XR configuration and deployment steps may change as the project setup evolves. Check the project's package configuration and Unity settings before building.

## Repository Structure

The project follows a standard Unity layout. The exact contents may change during development.

```text
VR-Physics-Lab/
├── Assets/
│   ├── Scenes/
│   ├── Scripts/
│   ├── Materials/
│   ├── Prefabs/
│   └── ...
├── Packages/
├── ProjectSettings/
└── README.md
```

## Development Roadmap

The roadmap below describes the intended direction of the prototype. It does not imply that planned work is already implemented.

- [x] Establish the core VR laboratory environment.
- [x] Develop and refine object interaction.
- [x] Implement the first force and motion experiments.
- [x] Add multiple experiments to the initial laboratory.
- [ ] Improve feedback and experiment instructions.
- [ ] Test usability and comfort on Meta Quest 3.
- [ ] Gather feedback and iterate on the learning experience.

## Contributing

Feedback, bug reports, and ideas are welcome.

If you would like to contribute:

1. Check the existing [Issues](https://github.com/Enestncs/VR-Physics-Lab/issues) before opening a new one.
2. Describe bugs with clear reproduction steps.
3. For feature suggestions, explain the learning or interaction problem the idea addresses.
4. If you plan to submit code, open an issue first to discuss the proposed change.

Please keep contributions focused, documented, and compatible with the project's current Unity version.

## Project Status and Scope

VR Physics Lab is an independent prototype under active development. Features, experiments, and technical decisions may change as the project is tested and refined.

The repository is intended to document the development process and provide a foundation for future experimentation with VR-based physics education.

## License

No open-source license has been selected for this repository yet. Until a license is added, all rights are reserved by the copyright holder. Contributions and reuse should not be assumed to be permitted.

---

**Built with Unity and a curiosity for learning through experimentation.**
