import SwiftUI

@main
struct ColimaStatusBarApp: App {
    @StateObject private var interactor = ColimaInteractor()
    
    var body: some Scene {
        MenuBarExtra("Colima Status", systemImage: interactor.isRunning() ? "shippingbox.fill" : "shippingbox") {
            switch interactor.status {
            case .Running:
                Button("Stop") { Task { await interactor.stopColima() } }.keyboardShortcut("s")
            case .Starting:
                Text("Colima is starting...")
            case .Stopped:
                Button("Start") { Task { await interactor.startColima() } }.keyboardShortcut("s")
            case .Stopping:
                Text("Colima is stopping...")
                
            }
            
            Divider()
            if (interactor.isRunning()) {
                
                let containers = interactor.getRunningContainers()
                if (containers.isEmpty) {
                    Text("No containers running")
                }
                else {
                    Text("Running containers")
                    ForEach(interactor.getRunningContainers(), id: \.name) { container in
                        Button("\(container.image): \(container.name)") {
                            NSPasteboard.general.clearContents()
                            NSPasteboard.general.setString(container.name, forType: .string)
                        }
                        .help("Copy name of container to clipboard")
                    }
                }
                
                Divider()
            }
            
            Button("Exit") { exit(EXIT_SUCCESS); }.keyboardShortcut("q")
        }
    }
}
