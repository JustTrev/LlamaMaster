# LlamaMaster 

## **Repo description** 

*Windows GUI for running llama.cpp across two PCs: a Master app to configure and launch llama-server with RPC GPU splitting, and a Slave app to host the RPC server. Auto-downloads the CUDA runtime you pick.*

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

* Windows 10+ .NET 8  
* NVIDIA GPU(s) with a driver that supports the chosen CUDA version  
* Both PCs are on the same network (you can use this externally if you know what you are doing).

### **Quick start**

1. On a spare PC, open **LlamaSlave**, click *Download / Ensure Runtime*, then click *Start RPC Server*.  


**RPC Server Slave:  \<- use this on any spare pc as a Slave RPC\!**

<img width="912" height="678" alt="Screenshot 2026-11-25 003510" src="https://github.com/user-attachments/assets/d03a26c7-c6dc-4711-b3e8-a4bfc575f941" /> 
<img width="911" height="680" alt="Screenshot 2026-11-25 003538" src="https://github.com/user-attachments/assets/f7084797-967c-4ab2-881b-4e558b4f8543" />
<img width="911" height="676" alt="Screenshot 2026-11-25 003606" src="https://github.com/user-attachments/assets/05f33385-0bc1-42fd-871f-041308a91481" />





2. On the main PC, open **LlamaMaster**, download a runtime, set the spare PC's IP and the split ratio, then click *Save & Launch*.  


**Master Server: \<- use this to host llama.cpp RPC server and connect to your slave RPCs\!**

<img width="1096" height="814" alt="Screenshot 2026-11-25 003324" src="https://github.com/user-attachments/assets/45501008-541e-47ae-82dc-a9a558a26288" />
<img width="1085" height="811" alt="Screenshot 2026-11-25 003410" src="https://github.com/user-attachments/assets/a215c16f-f037-4677-8228-b49cb3be768f" />
<img width="1086" height="812" alt="Screenshot 2026-11-25 003438" src="https://github.com/user-attachments/assets/4d53066e-5567-46f0-ae00-72ae1798b009" />



3. Open [http\://127.0.0.1:8080](http://127.0.0.1:8080)  in your browser.

RPC is a way to offload any active parameters across your local network between two pc's and there resources. Combining two (or more) GPU VRAM pools into a network based VRAM cache, giving you additional head room to run bigger models without them sitting in RAM (RTX 2070 ~ GTX 1070 ti = 16GB of available VRAM split across the network). 

## **Check before use!**

* **Task Manager:** to see how the models are loaded and split across multiple machines.
* **CUDA 13.5:** the dropdown offers 13.5, but it's unconfirmed that llama.cpp releases a zip with that exact name. If the download fails, adjust the version list to what's on the releases page.  
* **Update checker:** it's currently a placeholder that always says you're up to date, coming soon.  
* **Credits:** "Created by: xSoMuchPotx \+ Grok (xAI)".

* ## **Disclaimer**

Llama.cpp Master & Slave is an independent, community project. It is not affiliated with, endorsed by, or sponsored by llama.cpp, ggml-org, or NVIDIA. All product names and trademarks belong to their respective owners.

The software is provided "as is", without warranty of any kind. Use it at your own risk. The authors are not liable for any damage, data loss, hardware issues, or other problems arising from its use.

* **Third-party downloads:** the runtime manager downloads llama.cpp and CUDA runtime binaries from the official llama.cpp GitHub releases. Those files are not part of this project and are subject to their own licenses.  
* **Network security:** the RPC server accepts connections without authentication. Only run it on a trusted local network, and never expose its port to the internet.  
* **Models:** you are responsible for complying with the license of any model you load.
