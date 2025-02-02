import SwiftUI

@main
struct ColimaStatusBarApp: App {
    @State var running: Bool = false;
    
    var body: some Scene {
        MenuBarExtra("Colima Status", systemImage: running ? "shippingbox.fill" : "shippingbox") {
            if (running) {
                Button("Stop") {
                    running = false;
                }
            }
            
            if (!running) {
                Button("Start") {
                    running = true;
                }
            }
            
            Button("Exit") {
                exit(EXIT_SUCCESS);
            }
        }
    }
}
