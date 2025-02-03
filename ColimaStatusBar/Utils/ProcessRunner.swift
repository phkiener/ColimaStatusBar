import Foundation

enum ProcessError: Error {
    case failed
}

struct ProcessRunner {
    
    struct ProcessResult {
        var exitCode: Int32
        var output: String
    }
    
    static func run(program: String, args: [String]) async -> Int32 {
        do {
            let result = try await execute(program: program, args: args)
            return result.exitCode
        } catch {
            return 256
        }
    }
    
    static func getOutput(program: String, args: [String]) async -> String {
        do {
            let result = try await execute(program: program, args: args)
            return result.output
        } catch {
            return ""
        }
    }
    
    private static func execute(program: String, args: [String]) async throws -> ProcessResult {
        let process = Process()
        
        let shell = ProcessInfo.processInfo.environment["SHELL"] ?? "/bin/zsh"
        process.executableURL = URL(fileURLWithPath: shell)
        process.arguments = ["-l", "-c", "\(program) \(args.joined(separator: " "))"]
        
        let outputStream = Pipe()
        process.standardOutput = outputStream
        
        let errorStream = Pipe()
        process.standardError = errorStream

        do {
            try process.run()
            process.waitUntilExit()
            
            let output = try? outputStream.fileHandleForReading.readToEnd()
            _ = try? errorStream.fileHandleForReading.readToEnd()
            
            var outputText = ""
            if (output != nil) {
                outputText = String(decoding: output!, as: UTF8.self)
            }
            
            return ProcessResult(exitCode: process.terminationStatus, output: outputText)
        }
        catch {
            throw ProcessError.failed
        }
    }
}
