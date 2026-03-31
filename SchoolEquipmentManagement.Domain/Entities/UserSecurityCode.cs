using SchoolEquipmentManagement.Domain.Common;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Domain.Exceptions;

namespace SchoolEquipmentManagement.Domain.Entities
{
    public class UserSecurityCode : AuditableEntity
    {
        public int UserId { get; private set; }
        public ApplicationUser User { get; private set; }
        public UserSecurityCodePurpose Purpose { get; private set; }
        public string ChallengeToken { get; private set; }
        public string CodeHash { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime? ConsumedAt { get; private set; }

        private UserSecurityCode()
        {
            User = null!;
            ChallengeToken = null!;
            CodeHash = null!;
        }

        public UserSecurityCode(
            int userId,
            UserSecurityCodePurpose purpose,
            string challengeToken,
            string codeHash,
            DateTime expiresAt)
        {
            if (userId <= 0)
            {
                throw new DomainException("Не указан пользователь для кода подтверждения.");
            }

            if (string.IsNullOrWhiteSpace(challengeToken))
            {
                throw new DomainException("Не указан идентификатор запроса подтверждения.");
            }

            if (string.IsNullOrWhiteSpace(codeHash))
            {
                throw new DomainException("Не указан хеш кода подтверждения.");
            }

            UserId = userId;
            Purpose = purpose;
            ChallengeToken = challengeToken.Trim();
            CodeHash = codeHash.Trim();
            ExpiresAt = expiresAt;
        }

        public bool IsActive(DateTime utcNow)
        {
            return !ConsumedAt.HasValue && ExpiresAt > utcNow;
        }

        public void Consume()
        {
            if (!ConsumedAt.HasValue)
            {
                ConsumedAt = DateTime.UtcNow;
                MarkAsUpdated();
            }
        }
    }
}
