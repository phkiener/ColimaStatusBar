import Foundation

struct ProcessRunner {
    static func run(program: String, args: [String]) async -> Int32 {
        let process = Process()
        process.executableURL = URL(fileURLWithPath: "/bin/zsh")
        process.arguments = ["-l", "-c", "\(program) \(args.joined(separator: " "))"]
        
        let outputStream = Pipe()
        process.standardOutput = outputStream
        
        let errorStream = Pipe()
        process.standardError = errorStream

        do {
            try process.run()
            process.waitUntilExit()
            
            _ = try? outputStream.fileHandleForReading.readToEnd()
            _ = try? errorStream.fileHandleForReading.readToEnd()
            
            let exitCode = process.terminationStatus
            return exitCode
        }
        catch {
            return 255
        }
    }
}
