# LlamaMaster
# **Llama.cpp Master & Slave: GitHub Description**

Description, topics and README intro for the two WinForms tools that run llama.cpp across two PCs over RPC.

## **Repo description**

The short line under the repo name:

*Windows GUI for running llama.cpp across two PCs: a Master app to configure and launch llama-server with RPC GPU splitting, and a Slave app to host the RPC server. Auto-downloads the CUDA runtime you pick.*

## **Topics**

llama-cpp llama-server rpc cuda multi-gpu winforms dotnet local-llm gguf

## **README intro**

# **Llama.cpp Master & Slave**

A pair of .NET 8 Windows Forms apps that make it easy to split a llama.cpp model across two machines over RPC, without hand-editing .bat files.

**LlamaMaster** runs on your main PC. It's a visual designer and launcher for llama-server.

**LlamaSlave** runs on the spare PC. It's a lightweight front end for ggml-rpc-server with live output.

### **Features**

**Master**

* Runtime manager: choose a CUDA version and build tag, and it downloads the llama.cpp and cudart zips from GitHub releases, extracts them, and runs from that folder  
* Mixed CUDA versions work, since the main and RPC machines can each use a different one  
* RPC tensor split set as a ratio (e.g. 5:3 becomes \-ts 5,3)  
* Checkboxes turn each option on or off, so disabled values are left out of the command  
* Sliders for sampling (temperature, top-p, top-k, repeat penalty)  
* Folder browse buttons for the models folder and working directory  
* Runs llama-server inside the app, with no CMD window  
* Server output is shown in a colored log with clickable links  
* Can still generate and save a .bat script  
* Fully async, with a progress bar and cancelable downloads

**Slave**

* Same runtime downloader (e.g. CUDA 12.4 for older cards like the GTX 1070 Ti)  
* Start and stop the RPC server with live output  
* Configurable host, port, device and cache flag

**Both**

* Dark mode, persistent settings, menu bar (File / Settings / Help / About)

### **Requirements**

* Windows, .NET 8  
* NVIDIA GPU(s) with a driver that supports the chosen CUDA version  
* Both PCs on the same network

### **Quick start**

1. On the spare PC, open **LlamaSlave**, click *Download / Ensure Runtime*, then *Start RPC Server*.  
2. On the main PC, open **LlamaMaster**, download a runtime, set the spare PC's IP and the split ratio, then click *Save & Launch*.  
3. Open http\://127.0.0.1:8081 in your browser.

## **Check before you publish**

* **CUDA 13.5:** the dropdown offers 13.5, but it's unconfirmed that llama.cpp releases a zip with that exact name. If the download fails, adjust the version list to what's on the releases page.  
* **Update checker:** it's currently a placeholder that always says you're up to date, so don't list it as a working feature yet.  
* **Credits:** the About box says "Created by: You \+ Grok (xAI)". Put your own name or handle there and in the README.
