using FluentAssertions;
using LD.Messaging.Domain;
using Xunit;

namespace LD.Messaging.Adapter.Tests;

public class CheckedMonadTests
{
    // ── Construction ──────────────────────────────────────────────────────

    [Fact]
    public void Ok_produces_successful_check_with_correct_value()
    {
        var result = Checked<int>.Ok(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Fail_produces_failed_check_with_correct_error()
    {
        var result = Checked<int>.Fail("something went wrong");

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("something went wrong");
    }

    [Fact]
    public void Accessing_Value_on_failure_throws_InvalidOperationException()
    {
        var result = Checked<int>.Fail("oops");

        result.Invoking(r => _ = r.Value)
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*oops*");
    }

    [Fact]
    public void Accessing_Error_on_success_throws_InvalidOperationException()
    {
        var result = Checked<int>.Ok(1);

        result.Invoking(r => _ = r.Error)
            .Should().Throw<InvalidOperationException>();
    }

    // ── Bind ──────────────────────────────────────────────────────────────

    [Fact]
    public void Bind_on_success_applies_the_next_function()
    {
        var result = Checked<int>.Ok(5)
            .Bind(x => Checked<string>.Ok($"value is {x}"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value is 5");
    }

    [Fact]
    public void Bind_on_failure_short_circuits_without_calling_the_function()
    {
        var called = false;

        var result = Checked<int>.Fail("initial failure")
            .Bind(x =>
            {
                called = true;
                return Checked<string>.Ok("should not reach here");
            });

        called.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("initial failure");
    }

    [Fact]
    public void Chained_binds_propagate_the_first_failure()
    {
        var result = Checked<int>.Ok(1)
            .Bind(_ => Checked<int>.Fail("step 2 failed"))
            .Bind(_ => Checked<int>.Ok(99));   // never called

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("step 2 failed");
    }

    [Fact]
    public void Chained_binds_succeed_when_all_steps_pass()
    {
        var result = Checked<int>.Ok(1)
            .Bind(x => Checked<int>.Ok(x + 1))
            .Bind(x => Checked<int>.Ok(x * 10))
            .Bind(x => Checked<string>.Ok($"result={x}"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("result=20");
    }

    // ── Map ───────────────────────────────────────────────────────────────

    [Fact]
    public void Map_on_success_transforms_the_value()
    {
        var result = Checked<int>.Ok(7).Map(x => x * 2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(14);
    }

    [Fact]
    public void Map_on_failure_propagates_the_error()
    {
        var result = Checked<int>.Fail("fail").Map(x => x * 2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("fail");
    }

    // ── Match ─────────────────────────────────────────────────────────────

    [Fact]
    public void Match_action_calls_onSuccess_handler_for_successful_check()
    {
        var called = false;
        Checked<int>.Ok(3).Match(
            onSuccess: _ => called = true,
            onFailure: _ => { });
        called.Should().BeTrue();
    }

    [Fact]
    public void Match_action_calls_onFailure_handler_for_failed_check()
    {
        string? captured = null;
        Checked<int>.Fail("nope").Match(
            onSuccess: _ => { },
            onFailure: err => captured = err);
        captured.Should().Be("nope");
    }

    [Fact]
    public void Match_func_returns_success_projection()
    {
        var result = Checked<int>.Ok(10).Match(v => v + 1, _ => -1);
        result.Should().Be(11);
    }

    [Fact]
    public void Match_func_returns_failure_projection()
    {
        var result = Checked<int>.Fail("x").Match(_ => 0, _ => -1);
        result.Should().Be(-1);
    }

    // ── Extensions ────────────────────────────────────────────────────────

    [Fact]
    public void Tap_executes_side_effect_on_success_without_altering_the_check()
    {
        var sideEffect = 0;
        var result = Checked<int>.Ok(5).Tap(v => sideEffect = v);

        sideEffect.Should().Be(5);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public void Tap_does_not_execute_side_effect_on_failure()
    {
        var sideEffect = 0;
        Checked<int>.Fail("fail").Tap(_ => sideEffect = 99);
        sideEffect.Should().Be(0);
    }

    [Fact]
    public void TapError_executes_on_failure_and_passes_through()
    {
        string? captured = null;
        var result = Checked<int>.Fail("bad").TapError(e => captured = e);

        captured.Should().Be("bad");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void GetValueOrDefault_returns_value_on_success()
    {
        Checked<int>.Ok(42).GetValueOrDefault(0).Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_returns_default_on_failure()
    {
        Checked<int>.Fail("nope").GetValueOrDefault(-1).Should().Be(-1);
    }
}
