import Foundation
import SwiftUI
import Combine

enum Status { case Running, Starting, Stopped, Stopping }

@MainActor
class ColimaInteractor: ObservableObject {
    @Published var status: Status = Status.Stopped;
    private var cancellables = Set<AnyCancellable>()
    
    var isActive: Bool;
    
    init() {
        isActive = true;
        Timer.publish(every: 5, on: .main, in: .common)
            .autoconnect()
            .sink { [weak self] _ in Task { await self?.refresh() } }
            .store(in: &cancellables)
    }
    
    deinit {
        isActive = false;
    }
    
    func startColima() async {
        status = Status.Starting
        
        let exitCode = await ProcessRunner.run(program: "colima", args: ["start"])
        status = exitCode == 0 ? Status.Running : Status.Stopped
    }
    
    func stopColima() async {
        status = Status.Stopping
        
        let exitCode = await ProcessRunner.run(program: "colima", args: ["stop"])
        status = exitCode == 0 ? Status.Stopped : Status.Running
    }
    
    func isRunning() -> Bool { status == Status.Running }
    
    func isStopped() -> Bool { status == Status.Stopped }
    
    private func refresh() async {
        let exitCode = await ProcessRunner.run(program: "colima", args: ["status"])
        status = exitCode == 0 ? Status.Running : Status.Stopped
    }
}
