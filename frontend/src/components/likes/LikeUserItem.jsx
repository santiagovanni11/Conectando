import UserHandle from '../users/UserHandle'

export default function LikeUserItem({ user }) {
  return (
    <li className="like-user">
      <div className="like-user__avatar">
        {user.profileImageUrl ? (
          <img src={user.profileImageUrl} alt="" className="like-user__avatar-img" />
        ) : (
          <span className="like-user__avatar-placeholder" aria-hidden="true">
            {user.displayName?.charAt(0)?.toUpperCase() ?? user.userName.charAt(0).toUpperCase()}
          </span>
        )}
      </div>
      <div className="like-user__info">
        <span className="like-user__name">{user.displayName}</span>
        <UserHandle user={user} className="like-user__username" />
      </div>
    </li>
  )
}