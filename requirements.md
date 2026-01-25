You are a principal security tester at Microsoft with 15+ years of experience testing distributed systems, chat applications, and rate limiting implementations. Your job is to find vulnerabilities, edge cases, and breaking points that could be exploited by attackers or cause production incidents.
System Under Test:

PutZige Chat Application - Real-time messaging platform
Rate Limiting Implementation: ASP.NET Core built-in rate limiting

GlobalLimiter: Sliding Window (10,000/min per user in dev, 1,000/min in prod)
Login: Fixed Window (1,000/min dev, 5/15min prod)
Registration: Fixed Window (100/min dev, 3/hour prod)
RefreshToken: Fixed Window (1,000/min dev, 10/15min prod)


Partition Strategy: User ID (from JWT) for authenticated, IP for unauthenticated
Architecture: Clean Architecture, .NET 10, distributed cache support (Redis optional)

Attack Vectors to Test:
1. Boundary Gaming & Time-based Exploits

Window boundary exploitation (send requests at :59 and :00 seconds)
Clock skew attacks (server time manipulation)
Timezone exploitation
Leap second edge cases
Sliding window segment boundary attacks

2. Identity Spoofing & Bypass

IP spoofing via X-Forwarded-For header injection
JWT token manipulation to change User ID
Multiple tokens for same user (session hijacking)
Null/empty User ID exploitation
Special characters in User ID (SQL injection style)
IPv6 vs IPv4 switching to reset limits
VPN/proxy hopping to get new IPs

3. Distributed Attack Patterns

Distributed brute force (low rate from many IPs)
Slowloris-style slow requests to exhaust connections
Cache poisoning (Redis exploitation if enabled)
Race conditions in counter updates
Concurrent request floods (hit limit exactly at same millisecond)
Multi-threaded boundary testing

4. Resource Exhaustion

Memory exhaustion via unique partition keys
Redis connection pool exhaustion
Sliding window segment overflow
Partition key collision attacks
Large payload attacks combined with rate limiting

5. Configuration & State Manipulation

Disabled rate limiting bypass testing
Invalid configuration injection
Runtime configuration changes
Redis failover scenarios
In-memory vs distributed cache inconsistencies

6. Business Logic Exploitation

Account enumeration via differential responses
Timing attacks to distinguish valid/invalid users
Registration spam with disposable emails
Password reset flood attacks
Refresh token rotation exploitation

7. Edge Cases & Corner Cases

Exactly at limit boundary (1000th request)
Zero window duration
Negative permit limits
Overflow scenarios (int.MaxValue requests)
Expired vs active window edge cases
Multiple policies on same endpoint collision

Test Organization by Layer:
Layer 1: PutZige.Infrastructure.Tests/RateLimiting/ (NEW FOLDER)
File: RateLimitPartitioningTests.cs (Critical - Partition Logic)

GetPartitionKey_AuthenticatedUser_ReturnsUserId
GetPartitionKey_UnauthenticatedUser_ReturnsIpAddress
GetPartitionKey_XForwardedForHeader_UsesForwardedIp
GetPartitionKey_MultipleXForwardedForIps_UsesMostTrustedIp
GetPartitionKey_InvalidXForwardedFor_FallsBackToRemoteIp
GetPartitionKey_NullUserIdAndNullIp_ReturnsUnknown
GetPartitionKey_MaliciousXForwardedForInjection_Sanitized
GetPartitionKey_IPv6Address_NormalizedCorrectly
GetPartitionKey_IPv6WithPort_PortStripped
GetPartitionKey_UserIdWithSpecialCharacters_Sanitized

File: SlidingWindowLimiterTests.cs (Global API)
11. SlidingWindow_WithinLimit_AllowsRequests
12. SlidingWindow_ExceedsLimit_Returns429
13. SlidingWindow_AtExactLimit_1000thRequestAllowed_1001stDenied
14. SlidingWindow_WindowBoundary_NoGaming
15. SlidingWindow_SegmentRollover_CountsCorrectly
16. SlidingWindow_ConcurrentRequests_ThreadSafe
17. SlidingWindow_MultipleUsers_IndependentCounters
18. SlidingWindow_SameUserDifferentEndpoints_SharesCounter
19. SlidingWindow_WindowExpiry_ResetsCounter
20. SlidingWindow_PartialWindow_AllowsPartialRequests
File: FixedWindowLimiterTests.cs (Auth Endpoints)
21. FixedWindow_Login_5Attempts_6thDenied
22. FixedWindow_Login_WindowReset_AllowsNew5Attempts
23. FixedWindow_Login_BoundaryExploit_4At59Sec_2At00Sec_BlocksCorrectly
24. FixedWindow_Registration_3Attempts_4thDenied
25. FixedWindow_RefreshToken_10Attempts_11thDenied
26. FixedWindow_DifferentPolicies_IndependentCounters
27. FixedWindow_SameIpDifferentEndpoints_IndependentLimits
File: RedisDistributedCacheTests.cs (Production Scenarios)
28. Redis_Enabled_UsesDistributedCounter
29. Redis_ConnectionFailure_FallsBackToInMemory
30. Redis_KeyExpiry_ResetsCounterCorrectly
31. Redis_MultipleServers_ShareCounters
32. Redis_ConnectionPoolExhaustion_HandlesGracefully
33. Redis_PartitionKeyCollision_IsolatedCorrectly
34. Redis_NetworkLatency_DoesNotBlockRequests
Layer 2: PutZige.Application.Tests/RateLimiting/ (NEW FOLDER)
File: RateLimitSettingsValidatorTests.cs (Configuration Security)
35. Validate_ValidSettings_Passes
36. Validate_PermitLimitZero_Fails
37. Validate_PermitLimitNegative_Fails
38. Validate_PermitLimitOverflow_Fails
39. Validate_WindowSecondsZero_Fails
40. Validate_WindowSecondsNegative_Fails
41. Validate_WindowSecondsTooLarge_Fails
42. Validate_SegmentsPerWindowTooLow_Fails
43. Validate_SegmentsPerWindowTooHigh_Fails
44. Validate_AllPoliciesInvalid_ReturnsAllErrors
Layer 3: PutZige.API.Tests/Integration/RateLimiting/ (NEW FOLDER)
File: LoginRateLimitIntegrationTests.cs (Critical - Brute Force Protection)
45. Login_5FailedAttempts_6thReturns429
46. Login_5FailedAttempts_WaitForReset_AllowsNewAttempts
47. Login_4Attempts_SuccessfulLogin_CounterDoesNotReset (security: counter persists)
48. Login_RateLimitExceeded_RetryAfterHeaderPresent
49. Login_RateLimitExceeded_ResponseContainsCorrectRetryTime
50. Login_DifferentIPs_IndependentLimits
51. Login_SameIP_DifferentUsers_SharesLimit (unauthenticated = IP-based)
52. Login_XForwardedForSpoofing_UsesActualClientIP
53. Login_ConcurrentRequests_ThreadSafeCounter
54. Login_BoundaryAttack_SplitAcrossWindowBoundary_EnforcesLimit
File: RegistrationRateLimitIntegrationTests.cs (Spam Prevention)
55. Registration_3Accounts_4thReturns429
56. Registration_SpamWithDisposableEmails_LimitEnforced
57. Registration_DifferentIPs_IndependentLimits
58. Registration_SameIPMultipleAttempts_SharesLimit
59. Registration_RateLimitExceeded_ReturnsCorrectErrorMessage
60. Registration_VPNHopping_DetectsAndBlocks (if X-Forwarded-For tracking enabled)
File: RefreshTokenRateLimitIntegrationTests.cs (Token Rotation Attacks)
61. RefreshToken_10Attempts_11thReturns429
62. RefreshToken_SameUser_MultipleDevices_SharesLimit
63. RefreshToken_ExpiredToken_StillCountsTowardLimit
64. RefreshToken_RevokedToken_StillCountsTowardLimit
65. RefreshToken_RateLimitExceeded_DoesNotRotateToken
File: GlobalApiRateLimitIntegrationTests.cs (Chat Endpoints)
66. GlobalApi_1000Requests_AllSucceed
67. GlobalApi_1001Requests_LastReturns429
68. GlobalApi_SlidingWindow_SmoothDistribution_NoHarshCutoff
69. GlobalApi_BurstTraffic_50MessagesIn5Seconds_Allowed
70. GlobalApi_SustainedAbuse_2000RequestsIn60Sec_BlockedAt1000
71. GlobalApi_AuthenticatedUser_UsesUserId_NotIP
72. GlobalApi_MultipleEndpoints_SharesGlobalCounter
73. GlobalApi_SpecificPolicyOverride_DoesNotApplyGlobal
File: RateLimitBypassTests.cs (Security - Negative Testing)
74. Bypass_DisabledRateLimiting_AllowsUnlimitedRequests
75. Bypass_InvalidJWT_FallsBackToIPLimiting
76. Bypass_NoAuthHeader_UsesIPLimiting
77. Bypass_AdminRole_DoesNotBypassLimit (unless explicitly coded)
78. Bypass_SystemAccount_DoesNotBypassLimit
File: RateLimitEdgeCasesTests.cs (Corner Cases)
79. EdgeCase_ExactlyAtLimit_1000thRequestAllowed
80. EdgeCase_ConcurrentRequestsAtLimit_OnlyCorrectNumberAllowed
81. EdgeCase_WindowExpiry_DuringRequest_HandlesCorrectly
82. EdgeCase_ServerTimeChange_DoesNotResetCounters
83. EdgeCase_LeapSecond_DoesNotCauseError
84. EdgeCase_NegativeWindowDuration_Rejected
85. EdgeCase_ZeroPermitLimit_RejectsAllRequests
86. EdgeCase_IntMaxValueRequests_HandlesOverflow
File: RateLimitDistributedTests.cs (Multi-Server Scenarios)
87. Distributed_MultipleServers_ShareCountersViaRedis
88. Distributed_RedisDown_FallsBackToInMemory_LogsWarning
89. Distributed_RedisPartition_HandlesGracefully
90. Distributed_CacheMiss_DoesNotResetCounter
91. Distributed_ClockSkewBetweenServers_HandlesCorrectly
File: RateLimitPerformanceTests.cs (Load & Stress)
92. Performance_1000ConcurrentUsers_MaintainsLimits
93. Performance_SlidingWindow_MemoryUsage_AcceptableRange
94. Performance_HighThroughput_10KRequestsPerSecond_NoBottleneck
95. Performance_LongRunningTest_24Hours_NoMemoryLeak
File: RateLimitSecurityTests.cs (Attack Simulation)
96. Security_BruteForceLogin_BlockedAt5Attempts
97. Security_DistributedBruteForce_MultipleIPs_DetectedAndBlocked
98. Security_AccountEnumeration_ResponseTimingConsistent
99. Security_HeaderInjection_XForwardedFor_Sanitized
100. Security_SQLInjectionInUserId_Sanitized
101. Security_XSSInPartitionKey_Sanitized
102. Security_PasswordSpray_AcrossMultipleAccounts_Limited
File: RateLimitMonitoringTests.cs (Observability)
103. Monitoring_RateLimitHit_LogsStructuredWarning
104. Monitoring_ConfigurationLoaded_LogsInfo
105. Monitoring_RedisFailure_LogsError
106. Monitoring_RateLimitExceeded_IncludesPartitionKey
107. Monitoring_RateLimitExceeded_IncludesPolicyName
Layer 4: PutZige.API.Tests/Controllers/ (Existing - UPDATE)
File: AuthControllerTests.cs (UPDATE EXISTING)
108. Login_RateLimitExceeded_Returns429WithCorrectStatusCode
109. Login_RateLimitExceeded_ResponseMatchesSchema
110. RefreshToken_RateLimitExceeded_Returns429
File: UsersControllerTests.cs (UPDATE EXISTING)
111. CreateUser_RateLimitExceeded_Returns429
112. CreateUser_RegistrationSpam_BlockedAfter3Attempts
Test Categories Priority:
P0 (Critical - Must Have):

Tests 45-54 (Login rate limiting - brute force protection)
Tests 66-73 (Global API - chat functionality)
Tests 96-102 (Security attack simulation)

P1 (High - Security):

Tests 1-10 (Partition key logic)
Tests 11-20 (Sliding window correctness)
Tests 21-27 (Fixed window correctness)
Tests 55-60 (Registration spam)

P2 (Medium - Reliability):

Tests 28-34 (Redis distributed scenarios)
Tests 74-78 (Bypass scenarios)
Tests 79-86 (Edge cases)
Tests 87-91 (Multi-server)

P3 (Nice to Have - Performance & Monitoring):

Tests 92-95 (Performance)
Tests 103-107 (Monitoring)

Testing Standards:
Integration Tests (API.Tests):

Use WebApplicationFactory<Program>
Real HTTP requests via HttpClient
Real rate limiting middleware (not mocked)
Use in-memory database for test isolation
Configure test-specific rate limits (lower thresholds for faster tests)
Parallel test execution support (isolated partition keys)

Unit Tests (Infrastructure.Tests, Application.Tests):

Mock dependencies (IOptions, ILogger, Redis)
Test single responsibility
Fast execution (<100ms per test)
Deterministic (no time-based flakiness)

Security Tests:

Test actual attack vectors
Use realistic payloads
Verify logging of suspicious activity
Test defense in depth (multiple layers)

Performance Tests:

Measure memory under load
Measure response time degradation
Test concurrent access patterns
Identify bottlenecks

Code Quality:

Naming: MethodName_Scenario_ExpectedBehavior
Arrange-Act-Assert pattern
Use xUnit, FluentAssertions, Moq
XML comments on complex tests
Parameterized tests with [Theory] where appropriate
Test data builders for complex objects
Async/await properly
Dispose resources (WebApplicationFactory, HttpClient)

Deliverables:
For each test file, provide:

Complete test class with all test methods
Setup/teardown logic
Helper methods for common operations (e.g., SendLoginRequests(count))
Test data builders if needed
Comments explaining attack vectors for security tests

Output Format:
## LAYER 1: INFRASTRUCTURE.TESTS

### File: RateLimitPartitioningTests.cs
[Complete test class with tests 1-10]

### File: SlidingWindowLimiterTests.cs
[Complete test class with tests 11-20]

[Continue for all infrastructure tests...]

## LAYER 2: APPLICATION.TESTS

### File: RateLimitSettingsValidatorTests.cs
[Complete test class with tests 35-44]

## LAYER 3: API.TESTS (Integration)

### File: LoginRateLimitIntegrationTests.cs
[Complete test class with tests 45-54]

[Continue for all integration tests...]

## LAYER 4: API.TESTS (Controller Updates)

### File: AuthControllerTests.cs (UPDATE)
[Show new test methods to add: 108-110]

### File: UsersControllerTests.cs (UPDATE)
[Show new test methods to add: 111-112]

## TEST EXECUTION GUIDE

### How to run specific test categories:
[Commands for P0, P1, P2, P3]

### How to run security tests:
[Commands and setup]

### How to run performance tests:
[Commands and environment setup]

## KNOWN ATTACK VECTORS COVERED

[Summary of security vulnerabilities tested]

## ADDITIONAL SECURITY RECOMMENDATIONS

[Any additional testing needed beyond this suite]
Implement all 112 test cases as a principal security tester would. Think like an attacker trying to break the system. Be thorough, be paranoid, be comprehensive.

Test Distribution Summary:

Infrastructure.Tests: 34 tests (partition logic, limiters, Redis)
Application.Tests: 10 tests (configuration validation)
API.Tests/Integration: 63 tests (end-to-end scenarios, security, performance)
API.Tests/Controllers: 5 tests (controller updates)

Total: 112 comprehensive security-focused tests 🔒🚀
This is production-grade, Microsoft-level testing. Ship it! ✅