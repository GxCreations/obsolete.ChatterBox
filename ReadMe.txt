This file describes steps for building, running projects in ChatterBox.sln and using WebRTC ChatterBox application.

The ChatterBox.sln solution is contains 13 projects. 
There are references between these projects. For correct usage read following sections.


---Building---
There are 4 projects that could be built and run as top projects, others will built by references:
- ChatterBox.Server				- for server app
- ChatterBox.Universal.Client	- for win10 app
- ChatterBox.Client.Win8dot1	- for win8.1 app
- ChatterBox.Client.Console		- for console app.


---Running---
1. Run ChatterBox.Server on any machine
2. Run win10/win8.1 client application on the same/or other device.Note this will run BachgroundHost as well.
3. Open settings page and set Server's machin ip in "Server Host" field.
4. Save settings, go back and signin.


---Making call---
  If device is in portrate mode Peer should be choosen for opening view with audio/video buttons.
Audio and video calls may be made between 2 signed peers.
Call notifications should be received even if client is minimized.


---Messaging---
  If device is in portrate mode Peer should be choosen for opening chatview.
Peers can send chat messages each other.
All peers with unread messages should be highlighted in "Chatter Members" list

---Application Insights logging---
  User can switch On "AppInsight Logging" setting to send some metrics & logs to appInsights resource in Azure.
By default all metrics are sent to following resoucre:
https://ms.portal.azure.com/#resource/subscriptions/58581a18-1419-4c9d-9741-3ac843b57644/resourceGroups/Group-4/providers/microsoft.insights/components/WebRTC
But if one doesn't have access or wants to change resouce, can do it by changing AppInsights configuration or just updating <InstrumentationKey> in ChatterBox.Client.Universal/ApplicationInsights.config file.
  The following custom events are logged in AppInsights resource:
 - CallStarted - This event logged when incoming/outgoing call is started. It contains "Timestamp" and "Connection Type" custom data fields.
 - CallEnded - This event logged when call ended. It contain custom fields "Timestamp" and "Call Duration".
 - Network Average Quality	- This event logged after each call end. It shows "Timestamp", "Minimum Inbound Speed" and "Maximum Outbound Speed" during call. These are average speeds taken from network adapter every 20 scond during call. 
 - Audio Codec - This event logged in call start and during call if audio codec changed. It contains "Timestamp" and "Audio codec used for call" (codec name).
 - Video Codec - This event logged in call start and during call if video codec changed. It contains "Timestamp" and "Video codec used for call" (codec name).
 - Video Height/Width Downgrade (if it happens during call)	- This event logged during call if current video height/width downgraded during the call. Custom Fields are "Old Height/Width" and "New Height/Width".
 - Application Suspending/Resuming - This event is logged when application is suspended or resumed. It contains just "Timestamp" field.
 
  The following metrics can be found in the "Metrics Explorer" of the AppInsights resoucre :
 - Old/New Height/Width- These metrics are from "Video Height/Width Downgrade" event
 - Minimum Inbound/Maximum Outbound Speed - These metrics are taken from "Network Average Quality" event 
 - Audio/Video Packet Lost Rate -  The metric is logged once per minute during call. It is equal to <sum of sent audio/video packets>/<sum of lost audio/video packets> 
 - Audio/Video Current Delay Rate - The metric is logged after call end. It is equal to <average of audio/video current delays (received from webrtc)>.
 
 
 ---RTC Traceing---
 For collecting RTC Traces user needs to do following steps:
1. Run tcp server on remote machine and listen port 23. It can be for example Hercules tcp server for windows or SocketTest tcp server for mac.
2. Get the ip address of machine where tcp server is running (i.e. the PC's ip address). This address should be entered into the ChatterBox app UI (Step 4)
3. Switch On "RTC Trace" toggle buttn on settings page to enable tracing.
4. Fill Settings->"RTC Trace server Ip" with ip of server machine.
5. Set "RTC Trace server Port" to 55000.
6. Start Tcp client application and enter the IP address of the device and port 55000

 For more details about tracing look into:
 https://microsoft.sharepoint.com/teams/Real_Time_Communication_for_WinRT/_layouts/15/WopiFrame.aspx?sourcedoc={d0164c70-cb5a-4002-a3d3-91ac6f483d9b}&action=edit&wd=target%28%2F%2FTesting%20and%20Logging.one%7C80b51d33-b670-438e-9ba5-428191e0c57c%2FLogging%20and%20tracing%20for%20WebRTC%20sample%20apps%7C42c34c3d-8bab-4600-ab68-0128b159c000%2F%29