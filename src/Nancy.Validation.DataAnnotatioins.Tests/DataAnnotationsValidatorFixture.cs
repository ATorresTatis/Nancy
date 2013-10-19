﻿namespace Nancy.Validation.DataAnnotatioins.Tests
{
    using System.Linq;
    using FakeItEasy;
    using Nancy.Tests;
    using Nancy.Validation.DataAnnotations;
    using Xunit;

    public class DataAnnotationsValidatorFixture
    {
        public readonly IPropertyValidator propertyValidator1;
        public readonly IPropertyValidator propertyValidator2;
        public readonly IValidatableObjectAdapter validatableObjectAdapter;
        public readonly IPropertyValidatorFactory validatorFactory;
        public readonly DataAnnotationsValidator validator;

        public DataAnnotationsValidatorFixture()
        {
            this.propertyValidator1 =
                A.Fake<IPropertyValidator>();

            this.propertyValidator2 =
                A.Fake<IPropertyValidator>();

            this.validatableObjectAdapter =
                A.Fake<IValidatableObjectAdapter>();

            this.validatorFactory =
                A.Fake<IPropertyValidatorFactory>();

            A.CallTo(() => this.validatorFactory.GetValidators(typeof(ModelUnderTest)))
               .Returns(new[] { this.propertyValidator1, this.propertyValidator2 });

            this.validator =
                new DataAnnotationsValidator(typeof(ModelUnderTest), this.validatorFactory, this.validatableObjectAdapter);
        }

        [Fact]
        public void Should_get_property_validators_from_factory()
        {
            // Given
            var instance = new ModelUnderTest();

            // When
            this.validator.Validate(instance);

            // Then
            A.CallTo(() => this.validatorFactory.GetValidators(typeof(ModelUnderTest))).MustHaveHappened();
        }

        [Fact]
        public void Should_invoke_all_validators_returned_by_factory_with_instance_being_validated()
        {
            // Given
            var instance = new ModelUnderTest();

            // When
            this.validator.Validate(instance);

            // Then
            A.CallTo(() => this.propertyValidator1.Validate(instance)).MustHaveHappened();
            A.CallTo(() => this.propertyValidator2.Validate(instance)).MustHaveHappened();
        }

        [Fact]
        public void Should_invoke_validatable_object_adapter_with_instance_being_validated()
        {
            // Given
            var instance = new ModelUnderTest();

            // When
            this.validator.Validate(instance);

            // Then
            A.CallTo(() => this.validatableObjectAdapter.Validate(instance)).MustHaveHappened();
        }

        [Fact]
        public void Should_contain_validation_results_from_all_validators()
        {
            // Given
            var instance = new ModelUnderTest();

            var result1 = new ModelValidationError(string.Empty, x => string.Empty);
            var result2 = new ModelValidationError(string.Empty, x => string.Empty);
            var result3 = new ModelValidationError(string.Empty, x => string.Empty);

            A.CallTo(() => this.propertyValidator1.Validate(instance)).Returns(new[] { result1 });
            A.CallTo(() => this.propertyValidator2.Validate(instance)).Returns(new[] { result2, result3 });

            // When
            var results = this.validator.Validate(instance);

            // Then
            results.Errors.Count().ShouldEqual(3);
            results.Errors.Contains(result1).ShouldBeTrue();
            results.Errors.Contains(result2).ShouldBeTrue();
            results.Errors.Contains(result3).ShouldBeTrue();
        }

        [Fact]
        public void Should_contain_validation_result_from_validatable_object_adapter()
        {
            // Given
            var instance = new ModelUnderTest();
            var result = new ModelValidationError(string.Empty, x => string.Empty);

            A.CallTo(() => this.validatableObjectAdapter.Validate(instance)).Returns(new[] { result });

            // When
            var results = this.validator.Validate(instance);

            // Then
            results.Errors.Count().ShouldEqual(1);
            results.Errors.Contains(result).ShouldBeTrue();
        }

        [Fact]
        public void Should_return_descriptor_with_rules_from_all_validators()
        {
            // Given
            var rule1 = new ModelValidationRule(string.Empty, s => string.Empty);
            var rule2 = new ModelValidationRule(string.Empty, s => string.Empty);
            var rule3 = new ModelValidationRule(string.Empty, s => string.Empty);

            A.CallTo(() => this.propertyValidator1.GetRules()).Returns(new[] { rule1 });
            A.CallTo(() => this.propertyValidator2.GetRules()).Returns(new[] { rule2, rule3 });


            // When
            var descriptor = this.validator.Description;

            // Then
            descriptor.Rules.Count().ShouldEqual(3);
            descriptor.Rules.Contains(rule1).ShouldBeTrue();
            descriptor.Rules.Contains(rule2).ShouldBeTrue();
            descriptor.Rules.Contains(rule3).ShouldBeTrue();
        }

        public class ModelUnderTest
        {
        }
    }
}