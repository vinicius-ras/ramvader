# Description
RAMvader is a lightweight and powerful library which allows a process running on Windows-based systems to access (read/write) another processes' memory spaces.

It uses p/Invoke to perform low-level operations, while still allowing an application to leverage the full power of the high-level .NET libraries for using more complex resources (such as UI development through WPF, UWP, etc). The main, initial purpose of RAMvader is to provide a library that can be used in the development of game trainers ("hack tools"), but it might also fit other purposes, such as antivirus/security software, machine learning, benchmarking, and others.

RAMvader currently supports **.NET Framework 4.5** and superior, being able to run on both 32-bit and 64-bit modes.

# Quickstart

After installing the library (steps are described in [this page](https://github.com/vinicius-ras/ramvader/wiki/InstallingNuget) of our wiki), first thing you have to do is find the target process, whose memory will be the target for your read/write operations. There are several ways to do that. Two snippets that might help you finding the target process are shown below:

~~~
// NOTE: "Process" class is part of the "System.Diagnostics" namespace
// Print a list of all running processes (their names and PIDs)
foreach (Process p in Process.GetProcesses())
    Console.WriteLine($"{p.ProcessName} (PID = {p.Id})");

// Retrieve a reference to a process, given its PID
Process p = Process.GetProcessById(12345);
~~~

Having a reference to the Process you want to operate on, you can attach a [Target](https://vinicius-ras.github.io/ramvader/docs/API/class_r_a_mvader_1_1_target.html) to that process. [Target](https://vinicius-ras.github.io/ramvader/docs/API/class_r_a_mvader_1_1_target.html) is the class which provides methods for performing the reading/writing operations in the target process. Here's how you attach it:

~~~
// NOTE: "Target" class is part of the "RAMvader" namespace

// Use some method to retrieve a reference to the process you want to operate on
Process p = ...;

// Instantiate a Target object and attach it to the process
Target targ = new Target();
targ.AttachToProcess(p);
~~~

After attaching, you can begin performing I/O operations in the target process' memory space.

Writing operations can be performed simply by calling the [Target.WriteToTarget()](https://vinicius-ras.github.io/ramvader/docs/API/class_r_a_mvader_1_1_target.html#a5c3f0ff13a1d8cde8bf24aa4054b67f3) method, passing the memory address where you want to perform the write operation, and the data which should be written (data type is used for determining what kind of data should be written to the target process' memory). Examples:

~~~
// Write a DWORD (32-bit signed integer value) to the target process' memory address 0xAABBCCDD
targ.WriteToTarget(new AbsoluteMemoryAddress(0xAABBCCDD), (Int32)123);

// Write a Single (32-bit float value) to the target process' memory address 0x11223344
targ.WriteToTarget(new AbsoluteMemoryAddress(0x11223344), (Single)321.5f);
~~~

Reading operations work in a similar fashion, by calling the [Target.ReadFromTarget()](https://vinicius-ras.github.io/ramvader/docs/API/class_r_a_mvader_1_1_target.html#a97d5017388700d23446f2f23fa232acc) method, passing the memory address where you want to perform the read operation, and a reference to the variable which should be read (variable type determines what kind of data should be read from the target process' memory). Examples:

~~~
// Read a QWORD (64-bit signed integer) from the target process' memory address 0x12345678
Int64 myVar = 0;
targ.ReadFromTarget(new AbsoluteMemoryAddress(0x12345678), ref myVar);

// Read a Double (64-bit double precision floating-point number) from the target process' memory address 0x00FACADA
Double myVar = 0;
targ.ReadFromTarget(new AbsoluteMemoryAddress(0x00FACADA), ref myVar);
~~~

# User Guide and API Reference

For more details on how to use this library, please read our [Wiki pages](https://github.com/vinicius-ras/ramvader/wiki).

These pages provide a comprehensive and more detailed explanation on both the library's architecture and usage, which includes some more advanced features such as [injection of code and data](https://github.com/vinicius-ras/ramvader/wiki/LearnInjector) on the target process' memory space.

You can also check the [API Reference](https://vinicius-ras.github.io/ramvader/docs/API/) to find descriptions of the library's classes, methods, and other components.
