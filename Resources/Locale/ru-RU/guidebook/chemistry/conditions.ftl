reagent-effect-condition-guidebook-total-damage =
    { $max ->
        [2147483648] у него есть по крайней мере { NATURALFIXED($min, 2) } общего урона
       *[other]
            { $min ->
                [0] у него не более { NATURALFIXED($max, 2) } общего урона
               *[other] у него от { NATURALFIXED($min, 2) } до { NATURALFIXED($max, 2) } общего урона
            }
    }

reagent-effect-condition-guidebook-total-hunger =
    { $max ->
        [2147483648] у цели есть по крайней мере { NATURALFIXED($min, 2) } голода
       *[other]
            { $min ->
                [0] у цели не более { NATURALFIXED($max, 2) } голода
               *[other] у цели от { NATURALFIXED($min, 2) } до { NATURALFIXED($max, 2) } голода
            }
    }

reagent-effect-condition-guidebook-reagent-threshold =
    { $max ->
        [2147483648] у него есть по крайней мере { NATURALFIXED($min, 2) } ед. { $reagent }
       *[other]
            { $min ->
                [0] у него не более { NATURALFIXED($max, 2) } ед. { $reagent }
               *[other] у него от { NATURALFIXED($min, 2) } до { NATURALFIXED($max, 2) } ед. { $reagent }
            }
    }

reagent-effect-condition-guidebook-mob-state-condition = пациент в состоянии { $state }
reagent-effect-condition-guidebook-job-condition = должность цели — { $job }

reagent-effect-condition-guidebook-solution-temperature =
    температура раствора { $max ->
        [2147483648] не менее { NATURALFIXED($min, 2) } К
       *[other]
            { $min ->
                [0] не более { NATURALFIXED($max, 2) } К
               *[other] от { NATURALFIXED($min, 2) } К до { NATURALFIXED($max, 2) } К
            }
    }

reagent-effect-condition-guidebook-body-temperature =
    температура тела { $max ->
        [2147483648] не менее { NATURALFIXED($min, 2) } К
       *[other]
            { $min ->
                [0] не более { NATURALFIXED($max, 2) } К
               *[other] от { NATURALFIXED($min, 2) } К до { NATURALFIXED($max, 2) } К
            }
    }

reagent-effect-condition-guidebook-organ-type =
    орган метаболизма { $shouldhave ->
        [true] —
       *[false] — не
    } { INDEFINITE($name) } { $name } орган

reagent-effect-condition-guidebook-has-tag =
    цель { $invert ->
        [true] не имеет
       *[false] имеет
    } тег { $tag }

reagent-effect-condition-guidebook-blood-reagent-threshold =
    { $max ->
        [2147483648] в крови есть по крайней мере { NATURALFIXED($min, 2) } ед. { $reagent }
       *[other]
            { $min ->
                [0] в крови не более { NATURALFIXED($max, 2) } ед. { $reagent }
               *[other] в крови от { NATURALFIXED($min, 2) } до { NATURALFIXED($max, 2) } ед. { $reagent }
            }
    }

reagent-effect-condition-guidebook-this-reagent = этот реагент