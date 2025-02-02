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
                ControlGroup("mssql-local") {
                    Text("Bound to port: 1234")
                    Divider()
                    Button("Quit") {
                        print("Quitting mssql-local")
                    }
                }
                
                Divider()
            }
            Button("Exit") { exit(EXIT_SUCCESS); }.keyboardShortcut("q")
        }
    }
}
