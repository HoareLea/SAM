# Project Instructions

This repository may be worked on by different AI agents,
models, sessions, accounts, and computers.

Git and committed repository files are the authoritative project state.
Do not rely on previous conversation memory.

## Before starting work

Before making significant changes:

1. Read this `AGENTS.md`.
2. Read `PROJECT_PROGRESS.md`, and the PR record document and
   PR description of any open PR you are continuing.
3. Understand the relevant existing code, architecture, tests, and conventions.
4. Review the current Git branch and working tree.
5. Continue from the documented current state.

Prefer small, safe, targeted changes consistent with existing conventions.
Avoid unrelated refactoring unless it is required for the task.

## Project continuity

`PROJECT_PROGRESS.md` is updated only after a PR has merged.
During implementation, keep status/evidence on the PR branch in the
PR record document and PR description. After merge, fetch the base
branch and add the closeout entry, including the merge SHA, as a
direct docs-only commit on the base branch. Do not modify unrelated
stream entries.

The order of work is:

code + tests + evidence → final PR CI → merge →
`PROJECT_PROGRESS.md` closeout commit on the base branch.

Never change `PROJECT_PROGRESS.md` on a PR branch.

Do not record trivial actions such as opening files,
searching the repository, or reading documentation.

Keep the PR record document and `PROJECT_PROGRESS.md` concise and
sufficient for another AI agent on another computer to continue
without access to the current conversation.

While a PR is open, its record document must contain, and after merge
the `PROJECT_PROGRESS.md` closeout entry must contain:

- current status;
- work completed;
- important decisions and assumptions;
- files changed;
- tests, builds, checks, or validation performed and results;
- unresolved issues, risks, or blockers;
- exact recommended next step.

When updating `PROJECT_PROGRESS.md`, preserve still-relevant information
from previous sessions. Remove or replace information only when it is
obsolete, resolved, or superseded.

Before ending meaningful work, verify that the PR record document and
PR description (or, after merge, the `PROJECT_PROGRESS.md` closeout
entry) accurately represent the current repository state.

When appropriate, remind the user to commit and push changes before
switching computers, accounts, sessions, or AI agents.

Never assume conversation history, terminal history, generated files,
or uncommitted local changes will exist on another computer.