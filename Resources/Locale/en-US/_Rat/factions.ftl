# Faction Selection UI
rat-faction-selector-none = No Subfaction
rat-faction-selector-no-factions = No subfactions created or loaded. Use the command: factioncreate <name> <whitelist:true/false> <description> to create subfactions.

# A subfaction is an additional organization within the main faction
rat-faction-selector-select = Select a subfaction from the list
rat-faction-selector-no-subfaction = No subfaction
rat-faction-selector-whitelist-required = [color=yellow]Whitelist required[/color]
rat-faction-selector-invalid-index = Invalid subfaction index: { $idx } (total: { $total })
rat-faction-selector-reset = Subfaction reset.
rat-faction-selector-selected = Selected subfaction: { $factionName }

# Faction Commands
rat-faction-command-no-factions = No available factions. Connect to the server first.
rat-faction-command-available = Available factions:
rat-faction-command-none =   none - No faction
rat-faction-command-usage = Use: selectfaction <name> or selectfaction none
rat-faction-command-reset = Faction reset.
rat-faction-command-not-found = Faction '{ $factionName }' not found.
rat-faction-command-selected = Selected faction: { $factionName }

# Faction Examine
rat-faction-examine = [color=gold]Subfaction: { $faction }[/color]

# Admin Faction Commands
rat-faction-admin-no-factions = No factions in database.
rat-faction-admin-list-header = Factions:
rat-faction-admin-total = Total factions: { $count }
rat-faction-admin-created = Created faction '{ $name }' (whitelisted: { $whitelisted }).
rat-faction-admin-deleted = Deleted faction ID { $id }.
rat-faction-admin-delete-failed = Failed to delete faction ID { $id }. Faction may not exist.
rat-faction-admin-no-subfactions = No subfactions found.
rat-faction-admin-list-columns = ID | Name | Whitelisted | Description
rat-faction-admin-yes = yes
rat-faction-admin-no = no
rat-faction-admin-set-manager = Set '{ $playerName }' as manager of subfaction ID { $factionId }.
rat-faction-admin-remove-manager = Removed '{ $playerName }' as manager of subfaction ID { $factionId }.
rat-faction-admin-invalid-boolean = Invalid boolean value: { $value }
rat-faction-admin-invalid-id = Invalid subfaction ID: { $id }. Must be a number.
rat-faction-admin-player-not-found = Player '{ $playerName }' not found.
rat-faction-admin-faction-not-found = Failed to set manager for subfaction ID { $factionId }. Subfaction may not exist or player is already a manager.
rat-faction-admin-remove-failed = Failed to remove manager from subfaction ID { $factionId }. Subfaction or manager may not exist.
rat-faction-admin-use-factionlist = Use 'factionlist' to see subfaction IDs
