# Code Reviewer Agent - Quick Start

## What Is It?
A **strict, automated code reviewer** that analyzes your unstaged changes before commit/push. It enforces security, performance, testing, and documentation standards using industry best practices.

## When to Use

✅ **USE THIS AGENT WHEN:**
- You have unstaged/untraced changes ready to commit
- You want a thorough code review before pushing to remote
- You're adding security-sensitive code (auth, payments, data access)
- You've added new tests and want quality gate feedback
- You're onboarding and learning TaxTrack standards

❌ **DON'T USE WHEN:**
- You just want general coding help (use default Copilot)
- You need help with debugging (not its purpose)
- You're exploring code without committing yet

## Quick Commands

### Review Unstaged Changes
```
@CodeReviewer Review my unstaged changes before commit
```

### Review Specific Files
```
@CodeReviewer Review these files: src/Controllers/AccountController.cs, src/features/auth/LoginForm.tsx
```

### Review with Approval
```
@CodeReviewer Review these files and tell me if it's safe to commit: [file list]
```

### Review Frontend Only
```
@CodeReviewer Review all TypeScript/React changes in my working directory
```

### Review Backend Only
```
@CodeReviewer Review all C# changes in my working directory
```

## What You'll Get

The agent will return a **structured report** with:

```
📋 CODE REVIEW REPORT
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔒 SECURITY ISSUES (Critical)
  ❌ Line 42: Hardcoded API key detected
     Fix: Move to environment variable in appsettings.json

🧪 TESTING (High Priority)
  ⚠️  New method GetUserRiskScore() has no tests
     Fix: Add unit tests in RiskScoringTests.cs

🏗️  ARCHITECTURE (Medium Priority)
  ⚠️  Direct database query in Controller
     Fix: Move to Application layer service

✅ PASSED CHECKS
  ✓ No SQL injection vulnerabilities
  ✓ Proper error handling
  ✓ Valid TypeScript types

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
VERDICT: BLOCKED ❌
Reason: Critical security issue must be fixed before commit
```

## Approval Workflow

### If Agent Returns ✅ APPROVED
```
→ Your code is ready to commit
→ Run: git commit -m "feat: [description]"
→ Then: git push
```

### If Agent Returns ❌ BLOCKED
```
→ Fix the violations listed in the report
→ Run the agent again to re-review
→ Once approved, commit and push
```

### If You Disagree with Feedback
```
→ You can override BLOCKED status, but do so consciously
→ Add a comment in your commit: "Reviewed: override for [reason]"
→ This creates an audit trail
→ Never override Critical security issues
```

## Severity Levels Explained

| Severity | What It Means | Can You Commit? |
|----------|-------------|-----------------|
| **🔴 Critical** | Security/compliance violation | ❌ NO - Must fix |
| **🟠 High** | Design/testing failure | ⚠️ Check with teammate |
| **🟡 Medium** | Code quality/maintainability | ✅ Can commit, but not ideal |
| **🟢 Low** | Style/minor issues | ✅ Yes, context aware |

## Common Violations & Fixes

### 1. Hardcoded Secrets
**Violation:** `string apiKey = "sk-1234567890";`
**Fix:** Use configuration: `string apiKey = _config["API_KEY"];`

### 2. Missing Tests
**Violation:** New public method without unit tests
**Fix:** Add test in `[MethodName]Tests.cs` with happy and error cases

### 3. SQL Injection Risk
**Violation:** `new SqlCommand($"SELECT * FROM Users WHERE Id = {id}")`
**Fix:** Use parameterized queries: `await _dbContext.Users.FindAsync(id)`

### 4. N+1 Queries
**Violation:** Loop that hits database multiple times
**Fix:** Use `.Include()` or project before looping

### 5. Missing Documentation
**Violation:** Public API with no XML comments
**Fix:** Add `/// <summary>` XML doc for all public members

### 6. Code Too Complex
**Violation:** Method with 20+ branches/loops
**Fix:** Break into smaller methods with single responsibility

## Pre-Commit Best Practices

Run this checklist **before** invoking the agent:

- [ ] Tests pass locally: `dotnet test` or `npm test`
- [ ] No console.log() or Debug.WriteLine() calls left
- [ ] No commented-out code
- [ ] Configuration values moved to appsettings.json
- [ ] Database migrations written (if needed)
- [ ] Documentation updated (README, XML docs, etc.)

Then invoke the agent for the final quality gate.

## For Team Leads

### Measuring Code Quality
Track these metrics from agent reports:
- **Defect density:** Fewer bugs escaping to QA
- **Security incidents:** Target zero credential leaks
- **Test coverage:** Maintain >= 80% for new code
- **Review cycle time:** Average time from change to approval

### Using Feedback for Mentoring
- Share agent reports with junior team members
- Use violations as learning opportunities  
- Create team patterns from repeated issues
- Update coding standards if multiple people struggle

## Troubleshooting

**Q: Agent says "no unstaged changes found"**
A: Make sure you have unsaved modifications or unstaged files in git. The agent reads from `git status`.

**Q: Agent always blocks my commits**
A: It's doing its job! Review the violations carefully. If you feel it's too strict, discuss with the team lead about adjusting thresholds.

**Q: Can I ignore a warning?**
A: Only if you understand the risk. Low/Medium violations can be overridden with explanation. Never override Critical security issues.

**Q: How often should I run this?**
A: Every time before `git commit`. Make it a habit—catch issues early, not in QA.

---

## Agent Configuration

**Location:** `.github/agents/code-reviewer.agent.md`  
**Strictness:** HIGH (research-backed, non-negotiable security standards)  
**Languages:** C# (.NET), TypeScript/React, SQL  
**Scopes:** Backend (API, Application, Domain, Infrastructure), Frontend, Tests  

---

**Questions?** Check `docs/14-secure-coding-standards.md` or ask the DevOps team.
