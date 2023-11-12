# iiko-plugin
iiko plugin base
# What is done.
## Plugin
1. Open sync nameed pipe (Async more convinient for purpose, but not in 3 days with my skill)
2. Send table status change (order/ cancel order)
3. Can add reserve by button from plugin

## Application
1. Connect to plugin - and get tables
2. Wait for messages from server and change background of ListViewItem of table
3. Can send command to make reserve (do not work properly because of sync)
4. Stores only Table and status (without reserve period)

## Problems
1. Make proper echange system - async waiting for interchange command
2. Raserve data need to store in Application
3. Code in bad style unfortunately (Time too short with my skill, mach time spend to make it work)
