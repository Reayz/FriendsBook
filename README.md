# FriendsBook
A Social platform like FaceBook

---
This is a microservice application. There are following services:
1. Froentend Service - Full frontend for full application(Usasally there should be multiple frontend service, as a practise project I keep it together for simplicity)
2. Post Service - Add/Update/Delete posts
3. User Service - Authenticaion and user profile
4. Attachment Service - Add/Remove images

---
There has also muliple databases:
1. UserDB - User information
2. PostDB - Post related information

---
Uses following technologies:
1. ASP.Net Core 7
2. EF Core 7
3. MS SQL Server 2022
4. Docker - All services are running into docker container
5. RabbitMQ - Any changes to the User information will update to the PostDB

