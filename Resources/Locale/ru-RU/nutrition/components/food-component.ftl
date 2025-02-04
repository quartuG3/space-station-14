### Interaction Messages

food-you-need-to-hold-utensil = Вы должны держать { $utensil }, чтобы съесть это!
food-nom = Ням. { $flavors }
food-swallow = Вы проглатываете { $food }. { $flavors }
food-has-used-storage = Вы не можете съесть { $food } с хранящимся внутри предметом.
food-system-remove-mask = Сначала Вам нужно снять { $entity }.

## System

food-system-you-cannot-eat-any-more = В Вас больше не лезет!
food-system-you-cannot-eat-any-more-other = В н{ OBJECT($target) } больше не лезет!
food-system-try-use-food-is-empty = { CAPITALIZE($entity) } пустая!
food-system-wrong-utensil = Вы не можете есть { CAPITALIZE($food) } с помощью { $utensil }.
food-system-cant-digest = Вы не можете усвоить{ $entity }!
food-system-cant-digest-other = { CAPITALIZE(SUBJECT($target)) } не может усвоить { $entity }!
food-system-verb-eat = Съесть

## Force feeding

food-system-force-feed = { CAPITALIZE($user) } пытается Вам что-то скормить!
food-system-force-feed-success = { CAPITALIZE($user) } Вам что-то скормил! { $flavors }
food-system-force-feed-success-user = Вы успешно накормили { $target }
