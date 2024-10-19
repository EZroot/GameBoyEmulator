
# 🎮 GameBoyEmulator

![GameBoyEmulator Preview](https://raw.githubusercontent.com/EZroot/GameBoyEmulator/refs/heads/main/GameBoyEmulator/screenshots/gameboyemulator.gif)

## 📜 Description

**GameBoyEmulator** is a pure C# implementation of the original Game Boy hardware. It allows you to run Game Boy games in a Windows command-line environment, with support for key components like the CPU, memory, and graphics processing (PPU). ~~<small>This emulator focuses on accuracy and performance while maintaining a clean and readable codebase</small>~~, making it a great resource for anyone interested in emulation or CPU architecture.

## 🚀 Features

- 🕹️ **Game Boy ROM Support**: Runs standard 32KB Game Boy ROMs.
- 🧠 **CPU Emulation**: Partial instruction set coverage for the Game Boy's Z80-based CPU.
- 🎨 **Graphics (PPU)**: Rendering of the Game Boy's 160x144 pixel screen, including background and sprites.
- 🎮 **Input Handling**: Polling for joypad input for controlling in-game actions.
- 🖥️ **Command Line Interface**: Simple CLI for loading and playing games.
- 🛠️ **Debug Mode**: Step-through frame functionality for debugging game behavior.

## 🖼️ Preview

![Tetris Preview](https://raw.githubusercontent.com/EZroot/GameBoyEmulator/refs/heads/main/GameBoyEmulator/screenshots/gameboyemulator.gif)

*Screenshot of Tetris running on GameBoyEmulator*

## 🛠️ Getting Started

### Prerequisites

- 🟢 **.NET Core** 
- 🟢 **Any OS** 

### Installation

1. Clone the repository:

    ```bash
    git clone https://github.com/EZroot/GameBoyEmulator.git
    ```

2. Open the solution in **Visual Studio** or your preferred IDE.

3. Build the project to generate the executable.

### Running the Emulator

1. Place your **Game Boy ROM** files in the same directory as the emulator executable.

2. Run the emulator via the command line:

    ```bash
    GameBoyEmulator.exe path_to_rom.gb
    ```

### 🎮 Controls

- **Arrow Keys**: D-Pad (Up, Down, Left, Right)
- **Z**: A Button
- **X**: B Button
- **Spacebar**: Start
- **A**: Select

## 🤝 Contributing

If you'd like to contribute to this project, feel free to fork the repository and submit a pull request. Contributions for new features, performance improvements, and bug fixes are highly welcome!

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## 🙌 Acknowledgments

- Inspired by various **Game Boy emulators** and resources that explain the Game Boy hardware.
- Special thanks to the **open-source community** for providing a wealth of resources on emulation.
