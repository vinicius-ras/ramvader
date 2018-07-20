# RAMvader
RAMvader is a lightweight and powerful library which allows a process running on Windows-based systems to access (read/write) another process' memory space.

It uses p/Invoke to perform low-level operations, while still allowing an application to leverage the full power of the high-level .Net libraries for using more complex resources (such as UI development through WPF, UWP, etc). The main, initial purpose of RAMvader is to provide a library that can be used in the development of game trainers ("hack tools"), but it might also fit other purposes, such as antivirus/security software, machine learning, benchmarking, and others.

For more details on how to use this library, please read our [Wiki pages](https://github.com/vinicius-ras/ramvader/wiki).
