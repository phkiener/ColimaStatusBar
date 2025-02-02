import Foundation
import SwiftUI
import Combine

enum Status { case Running, Starting, Stopped, Stopping }

struct Container {
    let name: String;
    let image: String;
    let running: Bool;
}

@MainActor
class ColimaInteractor: ObservableObject {
    @Published var status: Status = Status.Stopped;
    @Published var containers: [Container] = []
    private var cancellables = Set<AnyCancellable>()
    private var isActive: Bool;
    
    init() {
        isActive = true;
        Timer.publish(every: 1, on: .main, in: .common)
            .autoconnect()
            .sink { [weak self] _ in Task { await self?.refresh() } }
            .store(in: &cancellables)
    }
    
    deinit {
        isActive = false;
        cancellables.forEach { c in
            c.cancel()
        }
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
    
    func getRunningContainers() -> [Container] {
        return containers.filter({ $0.running })
    }
    
    private func refresh() async {
        let exitCode = await ProcessRunner.run(program: "colima", args: ["status"])
        status = exitCode == 0 ? Status.Running : Status.Stopped
        
        if (status == Status.Running) {
            let lines = await ProcessRunner.getOutput(program: "docker", args: ["ps", "-a", "--no-trunc", "--format", "json"])
            lines.split(whereSeparator: \.isNewline).forEach { line in
                let container = parseContainerInfo(json: line.base)
                
                if let existingContainer = containers.first(where: { $0.name == container.name }) {
                    if (existingContainer.image != container.image || existingContainer.running != container.running) {
                        containers.removeAll(where: { $0.name == container.name })
                        containers.append(container)
                    }
                } else {
                    containers.append(container)
                }
            }
            
            containers = lines.split(whereSeparator: \.isNewline).map { parseContainerInfo(json: $0.base)}
        }
    }
    
    private func parseContainerInfo(json: String) -> Container {
        let parsed = try! JSONDecoder().decode(DockerOutput.self, from: json.data(using: .utf8)!)
        
        return Container(name: parsed.Names, image: parsed.Image, running: parsed.State == "exited" ? false : true)
    }
    
    private struct DockerOutput : Decodable {
        var Image: String
        var Names: String
        var State: String
    }
}
