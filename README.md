# COBOT-VR - A VR industrial assembly task designed to study the effects of collaborative robot (cobot) support on operator hesitation.
## Project: 101070596 — euROBIN — HORIZON-CL4-2021-DIGITAL-EMERGING-01
<img width="965" alt="Scherm­afbeelding 2025-01-24 om 13 20 37" src="https://github.com/user-attachments/assets/6b4018ca-7966-48ee-b01b-0a9c77099049" />

This repository contains the code and documentation for a Virtual Reality (VR) industrial assembly task designed to study the effects of collaborative robot (cobot) support on operator hesitation. The environment was built in Unity and leverages advanced hardware like HTC Vive Pro Eye and Manus XSense Gloves to provide immersive, interactive experiences with integrated eye-tracking.

## Project Overview

The VR scenario immerses users in a factory setting where they perform assembly tasks with varying levels of cobot assistance. By analyzing behavioral and physiological data, the setup identifies hesitation markers like gaze patterns to evaluate system support effectiveness.

## Features

- **Dynamic Cobot Support Levels**: The cobot offers low, medium, or high levels of assistance during assembly tasks.
- **Eye-Tracking Integration**: Real-time tracking of gaze patterns using the Vive Pro Eye and Sranipal SDK.
- **Data Logging**: Comprehensive logging of eye-tracking data, user interactions, and subjective task difficulty ratings.
- **VR Hardware Compatibility**: Built to run on HTC Vive Pro Eye and Manus XSense Gloves.
- **Customizable Virtual Environment**: Developed in Unity, allowing modifications to the scenario and interactions.

## Hardware Requirements

To use this VR scenario, the following hardware is required:

- **HTC Vive Pro Eye**: For VR visualization and eye-tracking.
- **Manus XSense Gloves**: For hand-tracking and interaction with the environment.
- **SteamVR Base Stations**: For precise tracking of VR hardware.

## Software Requirements

Ensure you have the following software installed:

- **Unity (2020 or later)**: The environment is built using Unity.
- **SteamVR**: To connect and manage VR hardware.
- **Sranipal SDK**: For eye-tracking data collection and integration.
- **Python 3**: For data preprocessing and analysis.
- **R with `lme4` package**: For statistical analysis of experimental data.

## Installation

1. Clone this repository:
   ```bash
   git clone https://github.com/your-repo/VR-Assembly-Cobot.git
   ```
2. Open the Unity project in Unity Editor (version 2020 or later).
3. Configure the VR hardware and ensure drivers are installed.
4. Run the project using the Unity play mode or build a standalone executable.

## Experiment Workflow

1. **Setup**:
   - Calibrate the eye tracker and familiarize participants with the VR environment.
   - Equip participants with HTC Vive Pro Eye and Manus XSense Gloves.

2. **Task Execution**:
   - Participants complete three assembly tasks, each with 13 steps.
   - Vary the cobot support level (low, medium, high) randomly across steps.

3. **Data Collection**:
   - Eye-tracking data (gaze patterns) logged in JSON format.
   - Screen recordings capture participant interactions.
   - Subjective difficulty ratings recorded after each step.

4. **Data Analysis**:
   - Use Python scripts for preprocessing eye-tracking data.
   - Perform statistical analysis in R using a linear mixed-effects model (LMM).

## Results and Insights

The VR scenario has been validated to:
- Identify hesitation through gaze-switching behavior.
- Evaluate the impact of cobot support levels on task difficulty and hesitation.
- Serve as a baseline for further research in human-robot collaboration and cognitive ergonomics.

## Acknowledgments

We thank Ward Dehairs for Unity development support. This project was carried out by imec-mict-UGent with contributions from [Jamil Joundi](mailto:jamil.joundi@ugent.be), [Jonas De Bruyne](mailto:jonas.debruyne@ugent.be), [Klaas Bombeke](mailto:klaas.bombeke@ugent.be)and others.



<img width="964" alt="Scherm­afbeelding 2025-01-24 om 13 18 02" src="https://github.com/user-attachments/assets/ae0eac74-5d43-489a-a819-7afa7c4a608e" />


<img width="965" alt="Scherm­afbeelding 2025-01-24 om 13 17 55" src="https://github.com/user-attachments/assets/49bd5f00-f004-41d9-a94a-e534b091e749" />




