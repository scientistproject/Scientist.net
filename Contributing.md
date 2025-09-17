# Contributing to Scientist.NET

## Table of Contents:
- [Contributing to Scientist.NET](#contributing-to-scientistnet)
  - [Table of Contents:](#table-of-contents)
  - [Code of conduct](#code-of-conduct)
  - [I just have a question](#i-just-have-a-question)
  - [How can i contribute?](#how-can-i-contribute)
    - [Reporting bugs](#reporting-bugs)
    - [Suggesting enhancements](#suggesting-enhancements)
    - [Your first code contribution](#your-first-code-contribution)
    - [Pull Requests](#pull-requests)
  - [Styleguides](#styleguides)
    - [Git commit messages](#git-commit-messages)
  - [Labels](#labels)
    - [Issue labels](#issue-labels)
    - [Topic category labels](#topic-category-labels)
    - [Pull request labels](#pull-request-labels)
  - [GitHub Actions](#github-actions)
    - [Running actions locally](#running-actions-locally)

## Code of conduct

This project and everyone participating in it is governed by the [Code of Conduct](CodeOfConduct.md). By participating, you are expected to uphold this code.

Please report unacceptable behavior on [Gitter](https://gitter.im/scientistproject/community) to a maintainer.

## I just have a question

> Please don't file an issue to ask a question.

> TODO


## How can i contribute?

### Reporting bugs

> TODO


### Suggesting enhancements

> TODO

### Your first code contribution
- [good first issue](https://github.com/scientistproject/Scientist.net/issues?q=is:issue+is:open+label:good+first+issue)

### Pull Requests

Please follow these steps to have your contribution considered by the maintainers:

- Follow all instructions in [the template](.github/PULL_REQUEST_TEMPLATE/pull_request_template.md)
- Follow the [styleguides](#styleguides)
- After you submit your pull request, verify that all [status checks](https://help.github.com/articles/about-status-checks/) are passing<details><summary>What if the status checks are failing?</summary>If a status check is failing, and you believe that the failure is unrelated to your change, please leave a comment on the pull request explaining why you believe the failure is unrelated. A maintainer will re-run the status check for you. If we conclude that the failure was a false positive, then we will open an issue to track that problem with our status check suite.</details>

While the prerequisites above must be satisfied prior to having your pull request reviewed, the reviewer(s) may ask you to complete additional design work, tests, or other changes before your pull request can be ultimately accepted.

## Styleguides
### Git commit messages
Please refer to [Conventional commits](https://www.conventionalcommits.org/en/v1.0.0/)

| Type             | Usage                                             | Version increment |
| ---------------- | ------------------------------------------------- | ----------------- |
| fix():           | A commit which fixes or patches a bug             | x.x.1             |
| feat():          | A commit which introduces a new feature           | x.1.x             |
| BREAKING CHANGE: | A commit which has BREAKING CHANGE: in the footer | 1.x.x             |

## Labels

### Issue labels
| Label name         | Search                                                                                                       | Description                                                                                       |
| ------------------ | ------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------- |
| `good first issue` | [search](https://github.com/scientistproject/Scientist.net/issues?q=is:issue+is:open+label:good+first+issue) | Less complex issues which would be good first issues to work on for users who want to contribute. |
| `help wanted`      | [search](https://github.com/scientistproject/Scientist.net/issues?q=is:issue+is:open+label:help+wanted)      | The would appreciate help from the community in resolving these issues.                           |

### Topic category labels
| Label name      | Search                                                                                                  | Description                           |
| --------------- | ------------------------------------------------------------------------------------------------------- | ------------------------------------- |
| `documentation` | [search](https://github.com/scientistproject/Scientist.net/issues?q=is:issue+is:open+label:)            | Related to any type of documentation. |
| `performance`   | [search](https://github.com/scientistproject/Scientist.net/issues?q=is:issue+is:open+label:performance) | Related to performance.               |
| `security`      | [search](https://github.com/scientistproject/Scientist.net/issues?q=is:issue+is:open+label:security)    | Related to security.                  |

### Pull request labels
| Label name         | Search                                                                                                       | Description                                                                              |
| ------------------ | ------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------- |
| `work-in-progress` | [search](https://github.com/scientistproject/Scientist.net/issues?q=is:issue+is:open+label:work-in-progress) | Pull requests which are still being worked on, more changes will follow.                 |
| `needs-review`     | [search](https://github.com/scientistproject/Scientist.net/issues?q=is:issue+is:open+label:needs-review)     | Pull requests which need code review, and approval from maintainers.                     |
| `under-review`     | [search](https://github.com/scientistproject/Scientist.net/issues?q=is:issue+is:open+label:under-review)     | Pull requests being reviewed by maintainers.                                             |
| `requires-changes` | [search](https://github.com/scientistproject/Scientist.net/issues?q=is:issue+is:open+label:requires-changes) | Pull requests which need to be updated based on review comments and then reviewed again. |
| `needs-testing`    | [search](https://github.com/scientistproject/Scientist.net/issues?q=is:issue+is:open+label:needs-testing)    | Pull requests which need manual testing.                                                 |


## GitHub Actions

### Running actions locally

[nektos/act](https://github.com/nektos/act)
`act workflow_dispatch -e payload.json`