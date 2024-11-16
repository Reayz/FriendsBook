# FriendsBook
A Microservice application as a social platform like Facebook

---
This is a microservice application. There are the following services:
1. Frontend Service - Full frontend part for the whole application(Usually there should be multiple frontend services)
2. Post Service - Add/Update/Delete posts
3. User Service - Authentication and user profile
4. Attachment Service - Add/Remove images

---
There are also multiple databases:
1. UserDB - User information
2. PostDB - Post related information

---
Uses the following technologies:
1. ASP.Net Core 7
2. EF Core 7
3. MS SQL Server 2022
4. Docker - All services are running into the docker container
5. RabbitMQ - Any changes to the User information will update to the PostDB
6. YARP - Reverse Proxy for service path forwarding
7. Serilog - For logging purposes
8. Prometheus, Grafana - For Matrix and Monitor purposes 
