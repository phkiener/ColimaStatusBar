import SwiftUI

@main
struct ColimaStatusBarApp: App {
    @State var running: Bool = false;
    
    var body: some Scene {
        MenuBarExtra("Colima Status", systemImage: running ? "shippingbox.fill" : "shippingbox") {
            Button(running ? "Stop" : "Start") { running = !running; }.keyboardShortcut("s")
            Divider()
            Button("Exit") { exit(EXIT_SUCCESS); }.keyboardShortcut("q")
        }
    }
}
