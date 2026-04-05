guidebook-reagent-effect-description =
    { $chance ->
        [1] { $effect }
       *[other] С вероятностью { NATURALPERCENT($chance, 2) } { $effect }
    }{ $conditionCount ->
        [0] .
       *[other] { " " }при { $conditions }.
    }

guidebook-reagent-name = [bold][color={ $color }]{ CAPITALIZE($name) }[/color][/bold]

guidebook-reagent-recipes-header = Рецепт
guidebook-reagent-recipes-reagent-display = [bold]{ $reagent }[/bold] \[{ $ratio }\]
guidebook-reagent-sources-header = Источники
guidebook-reagent-sources-ent-wrapper = [bold]{ $name }[/bold] \[1\]
guidebook-reagent-sources-gas-wrapper = [bold]{ $name } (газ)[/bold] \[1\]

guidebook-reagent-effects-header = Эффекты
guidebook-reagent-effects-metabolism-group-rate = [bold]{ $group }[/bold] [color=gray]({ $rate } единиц в секунду)[/color]

guidebook-reagent-plant-metabolisms-header = Метаболизм растений
guidebook-reagent-plant-metabolisms-rate = [bold]Метаболизм растений[/bold] [color=gray](1 единица каждые 3 секунды по умолчанию)[/color]

guidebook-reagent-physical-description = [italic]Похоже, что { $description }.[/italic]

guidebook-reagent-recipes-mix-info =
    { $minTemp ->
        [0]
            { $hasMax ->
                [true] { CAPITALIZE($verb) } ниже { NATURALFIXED($maxTemp, 2) }K
               *[false] { CAPITALIZE($verb) }
            }
       *[other]
            { CAPITALIZE($verb) } { $hasMax ->
                [true] между { NATURALFIXED($minTemp, 2) }K и { NATURALFIXED($maxTemp, 2) }K
               *[false] выше { NATURALFIXED($minTemp, 2) }K
            }
    }