using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Features.Enrollment.Commands.AssignTeacher;
using Domain.Entities.Academics;
using Domain.Entities.Enrollment;
using Domain.Entities.People;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class AssignTeacherCommandHandlerTests
    {
        private readonly FakeTeacherRepository _teachers = new();
        private readonly FakeClassRoomRepository _classRooms = new();
        private readonly FakeTeachingAssignmentRepository _assignments = new();
        private readonly AssignTeacherCommandHandler _handler;

        public AssignTeacherCommandHandlerTests()
        {
            _handler = new AssignTeacherCommandHandler(_assignments, _teachers, _classRooms);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenTeacherNotFound()
        {
            // Arrange
            var command = new AssignTeacherCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new List<ScheduleSlot>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("was not found", result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenClassRoomNotFound()
        {
            // Arrange
            var schoolId = Guid.NewGuid();
            var teacher = Teacher.Create(schoolId, Guid.NewGuid(), "EMP01", "John", "Doe");
            _teachers.Add(teacher);

            var command = new AssignTeacherCommand(
                schoolId, teacher.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new List<ScheduleSlot>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("was not found", result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenTeacherSchoolIdMismatch()
        {
            // Arrange
            var schoolId = Guid.NewGuid();
            var anotherSchoolId = Guid.NewGuid();
            var teacher = Teacher.Create(anotherSchoolId, Guid.NewGuid(), "EMP01", "John", "Doe");
            _teachers.Add(teacher);

            var command = new AssignTeacherCommand(
                schoolId, teacher.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new List<ScheduleSlot>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("was not found in this school", result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenClassRoomSchoolIdMismatch()
        {
            // Arrange
            var schoolId = Guid.NewGuid();
            var anotherSchoolId = Guid.NewGuid();
            var teacher = Teacher.Create(schoolId, Guid.NewGuid(), "EMP01", "John", "Doe");
            var classroom = ClassRoom.Create(anotherSchoolId, Guid.NewGuid(), Guid.NewGuid(), "Class A");
            _teachers.Add(teacher);
            _classRooms.Add(classroom);

            var command = new AssignTeacherCommand(
                schoolId, teacher.Id, Guid.NewGuid(), classroom.Id, Guid.NewGuid(), new List<ScheduleSlot>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("was not found in this school", result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenDuplicateAssignmentExists()
        {
            // Arrange
            var schoolId = Guid.NewGuid();
            var teacher = Teacher.Create(schoolId, Guid.NewGuid(), "EMP01", "John", "Doe");
            var classroom = ClassRoom.Create(schoolId, Guid.NewGuid(), Guid.NewGuid(), "Class A");
            _teachers.Add(teacher);
            _classRooms.Add(classroom);

            var termId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();
            _assignments.SetDuplicateExists(true);

            var command = new AssignTeacherCommand(
                schoolId, teacher.Id, subjectId, classroom.Id, termId, new List<ScheduleSlot>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("already assigned", result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenScheduleClashes()
        {
            // Arrange
            var schoolId = Guid.NewGuid();
            var teacher = Teacher.Create(schoolId, Guid.NewGuid(), "EMP01", "John", "Doe");
            var classroom = ClassRoom.Create(schoolId, Guid.NewGuid(), Guid.NewGuid(), "Class A");
            _teachers.Add(teacher);
            _classRooms.Add(classroom);

            var termId = Guid.NewGuid();
            var existingAssignment = TeachingAssignment.Create(schoolId, teacher.Id, Guid.NewGuid(), classroom.Id, termId);
            // Already scheduled: Monday 09:00 - 10:00
            existingAssignment.AddSchedule(Guid.NewGuid(), DayOfWeekEnum.Monday, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
            _assignments.Add(existingAssignment);

            // Requesting clash: Monday 09:30 - 10:30
            var newSlots = new List<ScheduleSlot>
            {
                new ScheduleSlot(DayOfWeekEnum.Monday, new TimeSpan(9, 30, 0), new TimeSpan(10, 30, 0), Guid.NewGuid())
            };

            var command = new AssignTeacherCommand(
                schoolId, teacher.Id, Guid.NewGuid(), classroom.Id, termId, newSlots);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("already scheduled on Monday", result.Error);
        }

        [Fact]
        public async Task Handle_ShouldSucceed_WhenAssignmentIsValid()
        {
            // Arrange
            var schoolId = Guid.NewGuid();
            var teacher = Teacher.Create(schoolId, Guid.NewGuid(), "EMP01", "John", "Doe");
            var classroom = ClassRoom.Create(schoolId, Guid.NewGuid(), Guid.NewGuid(), "Class A");
            _teachers.Add(teacher);
            _classRooms.Add(classroom);

            var termId = Guid.NewGuid();
            var slots = new List<ScheduleSlot>
            {
                new ScheduleSlot(DayOfWeekEnum.Monday, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), Guid.NewGuid()),
                new ScheduleSlot(DayOfWeekEnum.Wednesday, new TimeSpan(11, 0, 0), new TimeSpan(12, 0, 0), Guid.NewGuid())
            };

            var command = new AssignTeacherCommand(
                schoolId, teacher.Id, Guid.NewGuid(), classroom.Id, termId, slots);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
            Assert.Single(_assignments.SavedAssignments);
        }

        // FAKES FOR TESTING

        private class FakeTeacherRepository : ITeacherRepository
        {
            private readonly Dictionary<Guid, Teacher> _store = new();

            public void Add(Teacher teacher) => _store[teacher.Id] = teacher;

            public Task<Teacher?> GetByIdAsync(Guid id, CancellationToken ct = default)
            {
                _store.TryGetValue(id, out var teacher);
                return Task.FromResult(teacher);
            }

            public Task<bool> ExistsAsync(Guid schoolId, string employeeCode, Guid? excludingTeacherId = null, CancellationToken ct = default)
            {
                throw new NotImplementedException();
            }

            public Task AddAsync(Teacher teacher, CancellationToken ct = default)
            {
                Add(teacher);
                return Task.CompletedTask;
            }

            public Task RemoveAsync(Teacher teacher, CancellationToken ct = default)
            {
                return Task.CompletedTask;
            }
        }

        private class FakeClassRoomRepository : IClassRoomRepository
        {
            private readonly Dictionary<Guid, ClassRoom> _store = new();

            public void Add(ClassRoom classroom) => _store[classroom.Id] = classroom;

            public Task<ClassRoom?> GetByIdAsync(Guid id, CancellationToken ct = default)
            {
                _store.TryGetValue(id, out var classroom);
                return Task.FromResult(classroom);
            }

            public Task<bool> ExistsAsync(Guid schoolId, Guid gradeLevelId, Guid academicYearId, string name, CancellationToken ct = default)
            {
                throw new NotImplementedException();
            }

            public Task AddAsync(ClassRoom classRoom, CancellationToken ct = default)
            {
                Add(classRoom);
                return Task.CompletedTask;
            }
        }

        private class FakeTeachingAssignmentRepository : ITeachingAssignmentRepository
        {
            private readonly List<TeachingAssignment> _store = new();
            private bool _duplicateExists = false;

            public List<TeachingAssignment> SavedAssignments => _store;

            public void SetDuplicateExists(bool val) => _duplicateExists = val;

            public void Add(TeachingAssignment assignment) => _store.Add(assignment);

            public Task<TeachingAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
            {
                return Task.FromResult(_store.Find(a => a.Id == id));
            }

            public Task<bool> ExistsAsync(Guid schoolId, Guid teacherId, Guid classRoomId, Guid subjectId, Guid termId, CancellationToken ct = default)
            {
                return Task.FromResult(_duplicateExists);
            }

            public Task<IReadOnlyList<TeachingAssignment>> GetByTeacherAndTermAsync(Guid schoolId, Guid teacherId, Guid termId, CancellationToken ct = default)
            {
                IReadOnlyList<TeachingAssignment> result = _store.FindAll(a => a.SchoolId == schoolId && a.TeacherId == teacherId && a.TermId == termId);
                return Task.FromResult(result);
            }

            public Task AddAsync(TeachingAssignment assignment, CancellationToken ct = default)
            {
                Add(assignment);
                return Task.CompletedTask;
            }
        }
    }
}
