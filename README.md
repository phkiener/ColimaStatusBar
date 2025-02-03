# Colima StatusBar

A small menubar widget to show the status of [colima](https://github.com/abiosoft/colima).
Plus: You can start/stop the runtime as well as see all running containers.

It's basically just me dabbing into Swift and seeing what it's like.

## Notes

`colima` is invoked by running `$SHELL` (defaults to `/bin/zsh`). There is basically no
error-handling - if the runtime can't be started you're outta luck, there's no feedback.
