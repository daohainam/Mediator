alias b := build
alias c := clean
alias t := test

build:
    dotnet build Mediator.sln

clean:
    git clean -fxd
    dotnet build-server shutdown

_test constants:
    dotnet clean -v q
    dotnet build --no-restore -p:ExtraDefineConstants=\"{{constants}}\" -v q ./test/Mediator.Tests/
    dotnet test --no-restore --no-build ./test/Mediator.Tests/

test:
    dotnet restore -v m

    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_ForeachAwait'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_ForeachAwait'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_ForeachAwait'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_TaskWhenAll'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_TaskWhenAll'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_TaskWhenAll'

    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_ForeachAwait%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_ForeachAwait%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_ForeachAwait%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_TaskWhenAll%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_TaskWhenAll%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_TaskWhenAll%3BMediator_Large_Project'

    dotnet clean -v q
    dotnet build --no-restore -v q
    dotnet test --no-restore --no-build

test-transient:
    dotnet restore -v m

    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_ForeachAwait'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_TaskWhenAll'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_ForeachAwait%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Transient%3BMediator_Publisher_TaskWhenAll%3BMediator_Large_Project'

test-scoped:
    dotnet restore -v m

    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_ForeachAwait'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_TaskWhenAll'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_ForeachAwait%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Scoped%3BMediator_Publisher_TaskWhenAll%3BMediator_Large_Project'

test-singleton:
    dotnet restore -v m

    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_ForeachAwait'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_TaskWhenAll'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_ForeachAwait%3BMediator_Large_Project'
    just -f '{{ justfile() }}' _test 'Mediator_Lifetime_Singleton%3BMediator_Publisher_TaskWhenAll%3BMediator_Large_Project'
