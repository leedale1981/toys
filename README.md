# Toy Projects
I asked ChatGPT for a list of toy projects to make me a better software engineer, I will attempt to implement them in various languages and frameworks.

## 1. Core Computer Science & Algorithms

- Build a toy database engine: A key–value store with persistence to disk. Adds exposure to indexing, file I/O, and serialization.

- Write a regex engine: Implement pattern matching with finite automata. Teaches parsing and performance.

- Implement a garbage collector: In a toy language or runtime. Helps you really understand memory management.

## 2. Systems & Low-Level

- Simple operating system kernel: Handle interrupts, memory, and processes. Even “bare metal hello world” sharpens fundamentals.

- TCP/IP stack from scratch: A minimal implementation over raw sockets. Gives you deep networking insight.

- Build a CLI shell: Parse commands, implement pipes, and background jobs.

## 3. Web & Distributed Systems

- HTTP server from scratch: No frameworks. Just sockets, parsing requests, and serving responses.

- Chat server: Over WebSockets or TCP. Add scaling and resilience later.

- API gateway with rate limiting: Mimics what big systems use. Add authentication and logging.

## 4. Security & Cryptography

- JWT implementation: Roll your own token signer/verifier. Good for security practices.

- TLS handshake simulator: Implement handshake logic with simplified crypto.

- Post-quantum demo: Use Kyber/Dilithium libraries in a toy PKI system.

## 5. Data & Machine Learning

- Search engine: Crawl a small dataset, build an inverted index, implement ranking.

- Neural net from scratch: Forward/backprop in just numpy. Teaches ML internals.

- Recommendation system: Collaborative filtering for, say, books or movies.

## 6. Language & Compilers

- Toy programming language: Even a calculator language with a parser and interpreter.

- Bytecode VM: Define instructions, run programs.

- Linter/formatter: Parses AST and enforces style rules.

## 7. Developer Productivity Tools

- Static site generator: Markdown to HTML, templating, themes.

- Build tool: Like make but simpler, resolving dependencies and tasks.

- Code coverage tool: Parse coverage info, generate reports.

## 8. Real-World Simulation

- Banking system simulator: Accounts, transfers, fraud detection logic.

- Mini Git clone: Version files, track changes, branch/merge.

- Scheduler: Like cron — runs tasks at intervals with persistence.

## 9. Game Engine Core

- Implement a 2D game loop (update + render).

- Add an entity system, collision detection, sprite rendering.

- Optional: plug in a scripting layer (Lua, C#).

- Skills: graphics pipeline basics, real-time systems, modular design.

## 10. Physics Simulation Library

- Implement rigid body dynamics with gravity and friction.

- Add collision detection and resolution (AABB, SAT).

- Extend to constraints (springs, joints).

- Skills: applied math, linear algebra, numerical stability, API design.

## 11. Finance / Quant Dev

- C++ / Java / Python mastery	Write production-grade code using STL, templates, RAII, and smart pointers in C++. Deepen concurrency models (threads, async, atomics).
- Implement a mini-exchange simulator in C++ with order book matching (limit/market orders, FIFO queue).
- Re-implement parts of the simulator in Java (for GC benchmarking) and Python (for prototyping).
- Low-latency systems	Learn to reason about cache locality, branch prediction, and lock-free data structures.
- Write a fixed-size ring buffer queue with std::atomic for inter-thread messaging.
- Compare performance against std::queue under contention.
- Networking	Get comfortable with TCP/UDP sockets, message serialization (FlatBuffers, Protobuf), and feed-handler design.
- Parse live market data (e.g., Binance WebSocket feed).
- Benchmark throughput & latency of your handler vs ZeroMQ or Kafka.
- Profiling & optimization	Know how to measure, not guess.
- Use perf, valgrind, gprof, or Visual Studio profiler to analyze hotspots.
- Show before/after optimizations with real metrics.
- Probability & Statistics	Be fluent with distributions, expectation, variance, correlation, hypothesis testing.	
- Implement a Monte Carlo pricer for European options in Python and C++.
- Calibrate a Gaussian model to synthetic returns.
- Linear Algebra	Understand matrix ops, eigenvalues, SVD, covariance matrices.
- Implement PCA on historical stock returns to find key market factors.
- Time-series analysis	Use ARIMA, GARCH, and Kalman filters.
- Backtest a volatility-targeting strategy using Python (pandas, statsmodels).
- Optimization	Practice numerical optimization methods (gradient descent, Nelder–Mead, etc.).
