-create-3rd-person =
    { $chance ->
        [1] Создаёт
        *[other] создаёт
    } [web:17]

-cause-3rd-person =
    { $chance ->
        [1] Вызывает
        *[other] вызывает
    }

-satiate-3rd-person =
    { $chance ->
        [1] Утоляет
        *[other] утоляет
    }

reagent-effect-guidebook-create-entity-reaction-effect =
    { $chance ->
        [1] Создаёт
        *[other] создаёт
    } { $amount ->
        [1] {INDEFINITE($entname)}
        *[other] {$amount} {MAKEPLURAL($entname)}
    }

reagent-effect-guidebook-explosion-reaction-effect =
    { $chance ->
        [1] Вызывает
        *[other] вызывает
    } взрыв

reagent-effect-guidebook-emp-reaction-effect =
    { $chance ->
        [1] Вызывает
        *[other] вызывает
    } электромагнитный импульс

reagent-effect-guidebook-foam-area-reaction-effect =
    { $chance ->
        [1] Создаёт
        *[other] создаёт
    } большое количество пены

reagent-effect-guidebook-smoke-area-reaction-effect =
    { $chance ->
        [1] Создаёт
        *[other] создаёт
    } большое количество дыма

reagent-effect-guidebook-satiate-thirst =
    { $chance ->
        [1] Утоляет
        *[other] утоляет
    } { $relative ->
        [1] жажду со средней эффективностью
        *[other] жажду с эффективностью в {NATURALFIXED($relative, 3)}x от средней
    }

reagent-effect-guidebook-satiate-hunger =
    { $chance ->
        [1] Утоляет
        *[other] утоляет
    } { $relative ->
        [1] голод со средней эффективностью
        *[other] голод с эффективностью в {NATURALFIXED($relative, 3)}x от средней
    }

reagent-effect-guidebook-health-change =
    { $chance ->
        [1] { $healsordeals ->
                [heals] Лечит
                [deals] Наносит
                *[both] Изменяет здоровье на
             }
        *[other] { $healsordeals ->
                    [heals] лечит
                    [deals] наносит
                    *[both] изменяет здоровье на
                 }
    } { $changes }

reagent-effect-guidebook-status-effect =
    { $type ->
        [add]
            { $chance ->
                [1] Вызывает
               *[other] вызывают
            } { LOC($key) } минимум на { NATURALFIXED($time, 3) }, эффект накапливается
       *[set]
            { $chance ->
                [1] Вызывает
               *[other] вызывают
            } { LOC($key) } минимум на { NATURALFIXED($time, 3) }, эффект не накапливается
        [remove]
            { $chance ->
                [1] Удаляет
               *[other] удаляют
            } { NATURALFIXED($time, 3) } от { LOC($key) }
    }

reagent-effect-guidebook-activate-artifact =
    { $chance ->
        [1] Пытается
        *[other] пытается
    } активировать артефакт

reagent-effect-guidebook-set-solution-temperature-effect =
    { $chance ->
        [1] Устанавливает
        *[other] устанавливает
    } температуру раствора ровно на {NATURALFIXED($temperature, 2)}k

reagent-effect-guidebook-adjust-solution-temperature-effect =
    { $chance ->
        [1] { $deltasign ->
                [1] Добавляет
                *[-1] Убирает
            }
        *[other]
            { $deltasign ->
                [1] добавляет
                *[-1] убирает
            }
    } тепло { $deltasign ->
                [1] в
                *[-1] из
           } раствора, пока он не достигнет { $deltasign ->
                [1] не более {NATURALFIXED($maxtemp, 2)}k
                *[-1] не менее {NATURALFIXED($mintemp, 2)}k
            }

reagent-effect-guidebook-adjust-reagent-reagent =
    { $chance ->
        [1] { $deltasign ->
                [1] Добавляет
                *[-1] Убирает
            }
        *[other]
            { $deltasign ->
                [1] добавляет
                *[-1] убирает
            }
    } {NATURALFIXED($amount, 2)}u реагента {$reagent} { $deltasign ->
        [1] в
        *[-1] из
    } раствор

reagent-effect-guidebook-adjust-reagent-group =
    { $chance ->
        [1] { $deltasign ->
                [1] Добавляет
                *[-1] Убирает
            }
        *[other]
            { $deltasign ->
                [1] добавляет
                *[-1] убирает
            }
    } {NATURALFIXED($amount, 2)}u реагентов из группы {$group} { $deltasign ->
            [1] в
            *[-1] из
        } раствор

reagent-effect-guidebook-adjust-temperature =
    { $chance ->
        [1] { $deltasign ->
                [1] Добавляет
                *[-1] Убирает
            }
        *[other]
            { $deltasign ->
                [1] добавляет
                *[-1] убирает
            }
    } {POWERJOULES($amount)} тепла { $deltasign ->
            [1] в
            *[-1] из
        } тела, в котором это находится

reagent-effect-guidebook-chem-cause-disease =
    { $chance ->
        [1] Вызывает
        *[other] вызывает
    } болезнь { $disease }

reagent-effect-guidebook-chem-cause-random-disease =
    { $chance ->
        [1] Вызывает
        *[other] вызывает
    } болезни { $diseases }

reagent-effect-guidebook-jittering =
    { $chance ->
        [1] Вызывает
        *[other] вызывает
    } дрожь

reagent-effect-guidebook-chem-clean-bloodstream =
    { $chance ->
        [1] Очищает
        *[other] очищает
    } кровь от других химикатов

reagent-effect-guidebook-cure-disease =
    { $chance ->
        [1] Лечит
        *[other] лечит
    } болезни

reagent-effect-guidebook-cure-eye-damage =
    { $chance ->
        [1] { $deltasign ->
                [1] Наносит
                *[-1] Лечит
            }
        *[other]
            { $deltasign ->
                [1] наносит
                *[-1] лечит
            }
    } урон глазам

reagent-effect-guidebook-chem-vomit =
    { $chance ->
        [1] Вызывает
        *[other] вызывает
    } рвоту

reagent-effect-guidebook-create-gas =
    { $chance ->
        [1] Создаёт
        *[other] создаёт
    } { $moles } { $moles ->
        [1] моль
        *[other] молей
    } газа { $gas }

reagent-effect-guidebook-drunk =
    { $chance ->
        [1] Вызывает
        *[other] вызывает
    } опьянение

reagent-effect-guidebook-electrocute =
    { $chance ->
        [1] Бьёт током
       *[other] бьют током
    } употребившего в течении { NATURALFIXED($time, 3) }

reagent-effect-guidebook-extinguish-reaction =
    { $chance ->
        [1] Тушит
        *[other] тушит
    } огонь

reagent-effect-guidebook-flammable-reaction =
    { $chance ->
        [1] Повышает
        *[other] повышает
    } воспламеняемость

reagent-effect-guidebook-ignite =
    { $chance ->
        [1] Поджигает
        *[other] поджигает
    } метаболизирующего

reagent-effect-guidebook-make-sentient =
    { $chance ->
        [1] Делает
        *[other] делает
    } метаболизирующего разумным

reagent-effect-guidebook-make-polymorph =
    { $chance ->
        [1] Превращает
        *[other] превращает
    } метаболизирующего в { $entityname }

reagent-effect-guidebook-modify-bleed-amount =
    { $chance ->
        [1] { $deltasign ->
                [1] Вызывает
                *[-1] Снижает
            }
        *[other] { $deltasign ->
                    [1] вызывает
                    *[-1] снижает
                 }
    } кровотечение

reagent-effect-guidebook-modify-blood-level =
    { $chance ->
        [1] { $deltasign ->
                [1] Повышает
                *[-1] Понижает
            }
        *[other] { $deltasign ->
                    [1] повышает
                    *[-1] понижает
                 }
    } уровень крови

reagent-effect-guidebook-paralyze =
    { $chance ->
        [1] Парализует
       *[other] парализуют
    } употребившего минимум на { NATURALFIXED($time, 3) }

reagent-effect-guidebook-movespeed-modifier =
    { $chance ->
        [1] Делает
       *[other] делают
    } скорость передвижения { NATURALFIXED($walkspeed, 3) }x от стандартной минимум на { NATURALFIXED($time, 3) }

reagent-effect-guidebook-reset-narcolepsy =
    { $chance ->
        [1] Временно подавляет
        *[other] временно подавляет
    } нарколепсию

reagent-effect-guidebook-wash-cream-pie-reaction =
    { $chance ->
        [1] Смывает
        *[other] смывает
    } кремовый пирог с лица

reagent-effect-guidebook-cure-zombie-infection =
    { $chance ->
        [1] Лечит
        *[other] лечит
    } текущую зомби-инфекцию

reagent-effect-guidebook-cause-zombie-infection =
    { $chance ->
        [1] Заражает
        *[other] заражает
    } зомби-инфекцией

reagent-effect-guidebook-innoculate-zombie-infection =
    { $chance ->
        [1] Лечит
        *[other] лечит
    } текущую зомби-инфекцию и даёт иммунитет к будущим заражениям

reagent-effect-guidebook-reduce-rotting =
    { $chance ->
        [1] Регенерирует
       *[other] регенерируют
    } { NATURALFIXED($time, 3) } { MANY("second", $time) } гниения

reagent-effect-guidebook-plant-attribute =
    { $chance ->
        [1] Изменяет
        *[other] изменяет
    } {$attribute} на [color={$colorName}]{$amount}[/color]

reagent-effect-guidebook-plant-cryoxadone =
    { $chance ->
        [1] Омолаживает
        *[other] омолаживает
    } растение (в зависимости от возраста растения и времени роста)

reagent-effect-guidebook-plant-phalanximine =
    { $chance ->
        [1] Делает
        *[other] делает
    } нежизнеспособное из-за мутаций растение снова жизнеспособным

reagent-effect-guidebook-plant-diethylamine =
    { $chance ->
        [1] Повышает
        *[other] повышает
    } срок жизни растения и/или базовое здоровье, с шансом 10% на каждое

reagent-effect-guidebook-plant-robust-harvest =
    { $chance ->
        [1] Повышает
        *[other] повышает
    } силу растения на {$increase} до максимума {$limit}. Когда сила достигнет {$seedlesstreshold}, растение теряет семена. Попытка повысить силу выше {$limit} может снизить урожайность с шансом 10%.

reagent-effect-guidebook-plant-seeds-add =
    { $chance ->
        [1] Восстанавливает
        *[other] восстанавливает
    } семена растения

reagent-effect-guidebook-plant-seeds-remove =
    { $chance ->
        [1] Удаляет
        *[other] удаляет
    } семена растения

reagent-effect-guidebook-missing =
    { $chance ->
        [1] Вызывает
        *[other] вызывает
    } неизвестный эффект, так как его ещё никто не описал

reagent-effect-guidebook-change-glimmer-reaction-effect =
    { $chance ->
        [1] Изменяет
        *[other] изменяет
    } значение «глиммера» на {$count} очков

reagent-effect-guidebook-chem-remove-psionic =
    { $chance ->
        [1] Убирает
        *[other] убирает
    } псионические способности

reagent-effect-guidebook-chem-reroll-psionic =
    { $chance ->
        [1] Даёт
        *[other] даёт
    } шанс получить другую псионическую способность

reagent-effect-guidebook-chem-restorereroll-psionic =
    { $chance ->
        [1] Восстанавливает
        *[other] восстанавливает
    } способность получать эффект от реагентов, «открывающих разум»

reagent-effect-guidebook-add-moodlet =
    Изменяет настроение на {$amount}
    { $timeout ->
        [0] бессрочно
        *[other] на {$timeout} секунд
    }

reagent-effect-guidebook-remove-moodlet =
    Убирает модификатор настроения «{$name}».

reagent-effect-guidebook-purge-moodlets =
    Убирает все активные непостоянные модификаторы настроения.

reagent-effect-guidebook-purify-evil = Очищает от злых сил

reagent-effect-guidebook-stamina-change =
    { $chance ->
        [1] { $deltasign ->
                [-1] Восстанавливает
                *[1] Наносит
            }
        *[other] { $deltasign ->
                    [-1] восстанавливает
                    *[1] наносит
                 }
    } {$amount} выносливости

# Shadowling
reagent-effect-guidebook-blind-non-sling =
    { $chance ->
        [1] Ослепляет любого
        *[other] ослепляет любого
    } не-шэдоулинга

reagent-effect-guidebook-heal-sling =
    { $chance ->
        [1] Лечит любого
        *[other] лечит любого
    } шэдоулинга и раба

reagent-effect-guidebook-add-to-chemicals =
    { $chance ->
        [1] { $deltasign ->
                [1] Добавляет
                *[-1] Убирает
            }
        *[other]
            { $deltasign ->
                [1] добавляет
                *[-1] убирает
            }
    } {NATURALFIXED($amount, 2)}u {$reagent} { $deltasign ->
        [1] в
        *[-1] из
    } раствор

reagent-effect-guidebook-crit-modifier =
    { $deltasign ->
        [-1] Понижает
       *[1] Повышает
    } критический порог на {$amount} пунктов