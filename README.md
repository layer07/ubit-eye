
 # μBiT::EYE MinerPulse
 ## _Observability tool for MicroBT WhatsMiner miners._
 [![N|Solid](https://i.imgur.com/EyqeszJ.png)](https://kernelriot.com)

 
 [![License: GPL-2.0-or-later](https://img.shields.io/badge/GPL2.0+-License-red)](https://github.com/layer07/ubit-eye/blob/main/LICENSE)
 
 uBiT::EYE MinerPulse is a lean backend for monitoring MicroBT WhatsMiner devices.  
 It runs fast on Windows and Linux with zero bloat.
 
 - **Fast Scanner:** Detects live miners in seconds.
 - **Secure Communication:** HTTPS & WSS.
 - **Minimal & No-Fuzz:** Static classes, minimal APIs, low abstraction.
 - **Extreme Performance:** Ultra-low latency, minimal overhead.
 
 ## Features
 
 - **Dynamic Config:**  
   Auto-generates a config file (`app.conf`) on first run.
 - **Real-Time Scanning:**  
   Constantly updates miner statuses.
 - **Secure Metrics:**  
   Exposes Prometheus metrics via secure WebSocket.
 - **Cross-Platform:**  
   Runs natively on Windows and Linux.
 
 ## Installation
 
 ### Prerequisites
 
 - [.NET 8+ SDK](https://dotnet.microsoft.com/download)
 - [Git](https://git-scm.com/)
 
 ### Clone and Build
 
 ```bash
 git clone https://github.com/layer07/ubit-eye.git
 cd ubit-eye
 dotnet build
 ```
 
 ## Configuration
 
 On first launch, MinerPulse auto-generates `app.conf` and prompts for:
 - Miner IP range (start and end)
 - HTTPS port (WEB_PORT_SSL; WSS port = HTTPS port + 1)
 
 Defaults are used if no input is provided.
 
 ## Usage
 
 Run the application. MinerPulse will:
 - Scan your network for live miners.
 - Serve real-time metrics over secure WebSocket.
 - Display clear, colored console output.
 - Host a WebServer
 - Creates a SQLite Instance
 - Hosts a Prometheus /metrics endpoint
 - Make your life easier!

## Contributions

I've been maitaining this alone, if you want to help, I will be really glad, there are basically no rules, just make whatever you write run fast. Just do OOP if you have an excuse to do so, ideally just go functional or procedural and static.
 
 ## License
 
 This project is licensed under GPL-2.0-or-later. See the [LICENSE](LICENSE) file for details.
