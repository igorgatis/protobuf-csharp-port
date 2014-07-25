protobuf-csharp-port
====================

This is enhanced version of https://code.google.com/p/protobuf-csharp-port/.

Branches
========

There are three kinds of branches:

 * **master** branch is verbatim copy of the original project (it even includes mercurial files).
 * **issueXX** branches are branches derived from master with changes to implement Issue XX from [list of issues](https://code.google.com/p/protobuf-csharp-port/issues/list)
   * [Issue78](https://code.google.com/p/protobuf-csharp-port/issues/detail?id=78): adds support for C# native types: decimal, DateTime, DateTimeOffset and Guid.
   * [Issue90](https://code.google.com/p/protobuf-csharp-port/issues/detail?id=90): protoc-gen-cs: protoc plugin for protobuf-csharp-port.

Updating master branch
======================

Make sure you have Mercurial installed (e.g. under Ubuntu run `sudo apt-get install mercurial`).

Run:

```
# Go to master branch.
git co master

# Make sure it has no changes.
git status

# Pull changes from https://code.google.com/p/protobuf-csharp-port/.
hg pull -u

# Commit and push.
git commit -a -m'Updated from root project.'
git push

```
