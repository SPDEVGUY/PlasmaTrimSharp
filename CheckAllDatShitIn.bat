@echo off
git config --global user.email "evan@wangle.it"
git config --global user.name "evan"
git add -A
git commit -m "Checked in."
git push origin master